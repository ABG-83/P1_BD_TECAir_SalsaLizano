import api from './api';
import { getAeropuertosLocales, getVuelosLocales, syncAeropuertosLocales } from '../database/db';

export interface Airport {
  airportId: number;
  name: string;
  location: string;
}

export interface Flight {
  flightNumber: string;
  num_Vuelo: number;
  hora_Salida: string;
  hora_Llegada: string;
  precio: number;
  origin: Airport;
  destination: Airport;
  estado: string;
}

const parseFlightNumber = (fn?: string | number): number => {
  if (typeof fn === 'number') return fn;
  const digits = (fn ?? '').match(/\d+/g)?.join('');
  return digits ? Number(digits) : 0;
};

const mapFlight = (f: any): Flight => ({
  flightNumber: f.flightNumber ?? f.codigo_vuelo ?? '',
  num_Vuelo: parseFlightNumber(f.flightNumber ?? f.codigo_vuelo),
  hora_Salida: f.departureTime ?? f.fecha_salida ?? '',
  hora_Llegada: f.arrivalTime ?? f.fecha_llegada ?? '',
  precio: f.price ?? f.precio ?? 0,
  origin: f.origin ?? { airportId: f.origen_id ?? 0, name: f.origen ?? '', location: '' },
  destination: f.destination ?? { airportId: f.destino_id ?? 0, name: f.destino ?? '', location: '' },
  estado: f.status ?? f.estado ?? 'Scheduled',
});

const mapAirport = (a: any): Airport => ({
  airportId: a.airportId ?? a.AirportId ?? 0,
  name:      a.name      ?? a.Name      ?? '',
  location:  a.location  ?? a.Location  ?? '',
});

export const flightService = {
  getAirports: async (): Promise<Airport[]> => {
    try {
      const res = await api.get('/airports');
      const data: any[] = Array.isArray(res.data) ? res.data : [];
      const mapped = data.map(mapAirport).filter(a => a.airportId > 0 && a.name !== '');
      if (mapped.length > 0) {
        syncAeropuertosLocales(data);
      }
      return mapped;
    } catch {
      return getAeropuertosLocales();
    }
  },

  search: async (originId: number, destinationId: number, fecha?: string): Promise<Flight[]> => {
    try {
      let url = `/flights/search?originId=${originId}&destinationId=${destinationId}`;
      if (fecha) url += `&fecha=${fecha}`;
      const res = await api.get(url);
      return res.data.map(mapFlight);
    } catch {
      const local = getVuelosLocales();
      return local
        .filter(v =>
          v.origen_id === originId &&
          v.destino_id === destinationId &&
          (!fecha || (v.fecha_salida ?? '').startsWith(fecha))
        )
        .map(mapFlight);
    }
  },
};
