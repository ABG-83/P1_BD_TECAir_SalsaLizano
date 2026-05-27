import { useCallback, useState } from 'react';
import api from '../services/api'; 
import type { Promotion, PromotionCreate } from '../types';

// ── 1. EXACT REPRESENTATION OF PromotionResponseDto ──
interface BackendPromotionDto {
  promotionId: number;
  price: number;
  startDate: string;
  endDate: string;
  image?: string;
  isActive: boolean;
  origin: {
    airportId: number;
    name: string;
    location: string;
  };
  destination: {
    airportId: number;
    name: string;
    location: string;
  };
}

// ── 2. DATA TRANSFORMATION & CLEAN MAPPING ──
/**
 * Maps the structured English nested DTO into the legacy Spanish snake_case frontend structure.
 */
const mapPromotion = (backendPromo: BackendPromotionDto | any): Promotion => {
  const id_Promocion = backendPromo.promotionId ?? backendPromo.PromotionId;
  const precio = backendPromo.price ?? backendPromo.Price ?? 0;
  const fecha_Inicio = backendPromo.startDate ?? backendPromo.StartDate;
  const fecha_Fin = backendPromo.endDate ?? backendPromo.EndDate;
  const imagen = backendPromo.image ?? backendPromo.Image;
  const estado_Activa = backendPromo.isActive ?? backendPromo.IsActive ?? false;

  // Resolve nested nodes safely handling casing differences
  const originNode = backendPromo.origin ?? backendPromo.Origin;
  const destinationNode = backendPromo.destination ?? backendPromo.Destination;

  const id_Aeropuerto_Origen = originNode?.airportId ?? originNode?.AirportId ?? 0;
  const id_Aeropuerto_Destino = destinationNode?.airportId ?? destinationNode?.AirportId ?? 0;

  const aeropuertoOrigen = {
    id_Aeropuerto: id_Aeropuerto_Origen,
    nombre: originNode?.name ?? originNode?.Name ?? `Aeropuerto #${id_Aeropuerto_Origen}`,
    ubicacion: originNode?.location ?? originNode?.Location ?? '',
  };

  const aeropuertoDestino = {
    id_Aeropuerto: id_Aeropuerto_Destino,
    nombre: destinationNode?.name ?? destinationNode?.Name ?? `Aeropuerto #${id_Aeropuerto_Destino}`,
    ubicacion: destinationNode?.location ?? destinationNode?.Location ?? '',
  };

  return {
    id_Promocion,
    precio,
    fecha_Inicio,
    fecha_Fin,
    imagen,
    estado_Activa,
    id_Aeropuerto_Origen,
    id_Aeropuerto_Destino,
    aeropuertoOrigen,
    aeropuertoDestino,
  };
};

// ── 3. NATIVE CUSTOM HOOK IMPLEMENTATION ──
export const usePromotions = () => {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetches only active promotions from the backend and automatically maps the schema.
   */
  const getActivePromotions = useCallback(async (): Promise<Promotion[]> => {
    setLoading(true);
    setError(null);

    try {
      const res = await api.get<BackendPromotionDto[]>('/promotions/active');
      
      // Perform automated transformation array mapping
      const mappedResults = res.data.map(mapPromotion);
      
      setPromotions(mappedResults);
      return mappedResults;
    } catch (err: any) {
      const message = err.response?.data?.title || 
                      err.message || 
                      'Failed to load active promotions. Check server connectivity.';
      setError(message);
      setPromotions([]);
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Fetches the complete directory of promotions.
   */
  const getAllPromotions = useCallback(async (): Promise<Promotion[]> => {
    setLoading(true);
    setError(null);

    try {
      const res = await api.get<BackendPromotionDto[]>('/promotions');
      const mappedResults = res.data.map(mapPromotion);
      setPromotions(mappedResults);
      return mappedResults;
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to retrieve all promotions.');
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Submits a new promotion definition to the backend database architecture.
   */
  const createPromotion = useCallback(async (dto: PromotionCreate): Promise<number> => {
    setLoading(true);
    setError(null);

    try {
      const res = await api.post('/promotions', dto);
      return res.data.id_Promocion ?? res.data;
    } catch (err: any) {
      setError(err.response?.data?.title || 'Could not process promotion generation.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Alters an existing promotion schema via structural PUT operation.
   */
  const updatePromotion = useCallback(async (id: number, dto: Partial<PromotionCreate>): Promise<void> => {
    setLoading(true);
    setError(null);

    try {
      await api.put(`/promotions/${id}`, dto);
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to sync modifications.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  /**
   * Erases a promotion instance permanently from the application core context.
   */
  const removePromotion = useCallback(async (id: number): Promise<void> => {
    setLoading(true);
    setError(null);

    try {
      await api.delete(`/promotions/${id}`);
    } catch (err: any) {
      setError(err.response?.data?.title || 'Destruction routine failed.');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    promotions,
    loading,
    error,
    getActivePromotions,
    getAllPromotions,
    createPromotion,
    updatePromotion,
    removePromotion,
    reset: () => {
      setPromotions([]);
      setError(null);
    },
  };
};

export default usePromotions;
