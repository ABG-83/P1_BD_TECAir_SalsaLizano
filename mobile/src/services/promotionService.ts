import api from './api';
import { getPromocionesLocales } from '../database/db';

export interface Promotion {
  promotionId: number;
  origin: { airportId: number; name: string; location: string };
  destination: { airportId: number; name: string; location: string };
  price: number;
  startDate: string;
  endDate: string;
  image?: string;
  isActive: boolean;
}

const mapPromotion = (r: any): Promotion => ({
  promotionId: r.promotionId ?? r.PromotionId ?? 0,
  price: Number(r.price ?? r.Price ?? 0),
  startDate: String(r.startDate ?? r.StartDate ?? ''),
  endDate: String(r.endDate ?? r.EndDate ?? ''),
  image: r.image ?? r.Image ?? undefined,
  isActive: r.isActive ?? r.IsActive ?? true,
  origin: {
    airportId: r.origin?.airportId ?? r.origin?.AirportId ?? r.Origin?.airportId ?? r.Origin?.AirportId ?? 0,
    name: r.origin?.name ?? r.origin?.Name ?? r.Origin?.name ?? r.Origin?.Name ?? '',
    location: r.origin?.location ?? r.origin?.Location ?? r.Origin?.location ?? r.Origin?.Location ?? '',
  },
  destination: {
    airportId: r.destination?.airportId ?? r.destination?.AirportId ?? r.Destination?.airportId ?? r.Destination?.AirportId ?? 0,
    name: r.destination?.name ?? r.destination?.Name ?? r.Destination?.name ?? r.Destination?.Name ?? '',
    location: r.destination?.location ?? r.destination?.Location ?? r.Destination?.location ?? r.Destination?.Location ?? '',
  },
});

export const promotionService = {
  getAll: async (): Promise<Promotion[]> => {
    try {
      const res = await api.get('/promotions/active');
      const data: any[] = Array.isArray(res.data) ? res.data : [];
      return data.map(mapPromotion);
    } catch {
      return getPromocionesLocales();
    }
  },
};
