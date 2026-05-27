import { useState, useEffect, useCallback } from 'react';
import {
  Table, Button, Modal, Form, Alert, Spinner,
  Row, Col, Badge, Card,
} from 'react-bootstrap';
import { useFlights } from '../../hooks/useFlights';
import { useAirports } from '../../hooks/useAirports';
import { useAirplanes } from '../../hooks/useAirplanes';
import { flightService } from '../../services/flightService';
import type { Flight, FlightCreate, FlightStatus } from '../../types';

// ─── Helpers ────────────────────────────────────────────────────────────────

const STATUS_LABELS: Record<FlightStatus, string> = {
  Scheduled: 'Programado',
  Boarding:  'Abordaje',
  Delayed:   'Retrasado',
  InAir:     'En Vuelo',
  Landed:    'Aterrizado',
  Cancelled: 'Cancelado',
};

const STATUS_BADGE: Record<FlightStatus, string> = {
  Scheduled: 'secondary',
  Boarding:  'success',
  Delayed:   'warning',
  InAir:     'primary',
  Landed:    'dark',
  Cancelled: 'danger',
};

const STATUS_OPTIONS: FlightStatus[] = [
  'Scheduled', 'Boarding', 'Delayed', 'InAir', 'Landed', 'Cancelled',
];

const EMPTY_FORM: FlightCreate = {
  flightNumber: '', hora_Salida: '', hora_Llegada: '', estado: 'Scheduled',
  matricula: '', id_Aeropuerto_Origen: 0, id_Aeropuerto_Destino: 0, precio: 0,
};

/** Devuelve true si faltan ≤ 60 minutos para la salida del vuelo */
const withinOneHour = (departureTime: string): boolean => {
  const diff = new Date(departureTime).getTime() - Date.now();
  return diff <= 60 * 60 * 1000 && diff > 0;
};

/** Tiempo restante hasta la salida, en formato legible */
const timeUntilDeparture = (departureTime: string): string => {
  const diff = new Date(departureTime).getTime() - Date.now();
  if (diff <= 0) return 'ya salió';
  const h = Math.floor(diff / 3600000);
  const m = Math.floor((diff % 3600000) / 60000);
  return h > 0 ? `${h}h ${m}m` : `${m} min`;
};

// ─── Tipos del DTO de cierre (espejado del backend) ─────────────────────────

interface CheckedInPassenger {
  checkInId: number;
  seat: string;
  boardingGate: string;
  fullName: string;
  email: string;
  baggageCount: number;
  baggageSurcharge: number;
}

interface FlightClosingData {
  flightNumber: string;
  status: string;
  departureTime: string;
  arrivalTime: string;
  totalPassengers: number;
  totalBaggages: number;
  passengers: CheckedInPassenger[];
}

// ─── Componente ─────────────────────────────────────────────────────────────

const FlightManagementView = () => {
  const {
    flights, loading, error,
    getAllFlights, createFlight, updateFlight, deleteFlight,
    reset,
  } = useFlights();
  const { airports, loading: airLoading } = useAirports();
  const { airplanes, loading: planeLoading } = useAirplanes();

  // Estado del modal CRUD
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Flight | null>(null);
  const [form, setForm] = useState<FlightCreate>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState<string | null>(null);

  // Estado del modal de CIERRE DE VUELO
  const [closeModal, setCloseModal] = useState(false);
  const [closingFlight, setClosingFlight] = useState<Flight | null>(null);
  const [finalList, setFinalList] = useState<FlightClosingData | null>(null);
  const [loadingFinalList, setLoadingFinalList] = useState(false);
  const [confirmingClose, setConfirmingClose] = useState(false);
  const [closeError, setCloseError] = useState('');
  const [closeSuccess, setCloseSuccess] = useState(false);

  useEffect(() => {
    void getAllFlights();
  }, [getAllFlights]);

  const airportName = (id: number) =>
    airports.find(a => a.airportId === id)?.name ?? `#${id}`;

  // ── CRUD ────────────────────────────────────────────────────────────────

  const openCreate = () => { setEditing(null); setForm(EMPTY_FORM); setShowModal(true); };
  const openEdit = (f: Flight) => {
    setEditing(f);
    setForm({
      flightNumber: f.flightNumber ?? String(f.num_Vuelo),
      hora_Salida: f.hora_Salida.slice(0, 16),
      hora_Llegada: f.hora_Llegada.slice(0, 16),
      estado: f.estado,
      matricula: f.matricula,
      id_Aeropuerto_Origen: f.id_Aeropuerto_Origen,
      id_Aeropuerto_Destino: f.id_Aeropuerto_Destino,
      precio: f.precio ?? 0,
    });
    setShowModal(true);
  };

  const handleSave = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editing) {
        await updateFlight(editing.flightNumber ?? String(editing.num_Vuelo), form);
      } else {
        await createFlight(form);
      }
      setShowModal(false);
      await getAllFlights();
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (flightNumber: string) => {
    if (!confirm('¿Eliminar este vuelo?')) return;
    setDeleting(flightNumber);
    try { await deleteFlight(flightNumber); }
    finally { setDeleting(null); }
  };

  const set = (field: keyof FlightCreate) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm(prev => ({
        ...prev,
        [field]: (field.startsWith('id_') || field === 'precio')
          ? Number(e.target.value)
          : e.target.value,
      }));

  // ── CIERRE DE VUELO ─────────────────────────────────────────────────────

  /**
   * Abre el modal de cierre para un vuelo en estado Boarding.
   * Pre-carga la lista oficial de pasajeros con check-in antes de confirmar.
   */
  const openCloseModal = useCallback(async (flight: Flight) => {
    setClosingFlight(flight);
    setCloseModal(true);
    setFinalList(null);
    setCloseError('');
    setCloseSuccess(false);
    setLoadingFinalList(true);
    try {
      const data = await flightService.getFinalList(
        flight.flightNumber ?? String(flight.num_Vuelo)
      );
      setFinalList(data as FlightClosingData);
    } catch {
      setCloseError('No se pudo cargar la lista de pasajeros. Verifique que el vuelo esté en estado Boarding.');
    } finally {
      setLoadingFinalList(false);
    }
  }, []);

  /**
   * Confirma el cierre del vuelo.
   * Llama a PUT /api/flight-closing/{flightNumber}/close y refresca la tabla.
   */
  const handleConfirmClose = async () => {
    if (!closingFlight) return;
    setConfirmingClose(true);
    setCloseError('');
    try {
      const data = await flightService.closeFlight(
        closingFlight.flightNumber ?? String(closingFlight.num_Vuelo)
      );
      setFinalList(data as FlightClosingData);
      setCloseSuccess(true);
      await getAllFlights();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })
        ?.response?.data?.message;
      setCloseError(msg ?? 'No se pudo cerrar el vuelo. Intente nuevamente.');
    } finally {
      setConfirmingClose(false);
    }
  };

  const handleCloseModalDismiss = () => {
    setCloseModal(false);
    setClosingFlight(null);
    setFinalList(null);
    setCloseError('');
    setCloseSuccess(false);
  };

  // ── Render ───────────────────────────────────────────────────────────────

  return (
    <>
      {/* ── Encabezado ── */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 className="fw-bold mb-1">Vuelos</h3>
          <p className="text-muted mb-0">Administración de vuelos programados.</p>
        </div>
        <Button variant="dark" onClick={openCreate}>
          <i className="bi bi-plus-lg me-2" />Nuevo Vuelo
        </Button>
      </div>

      {error && <Alert variant="danger" dismissible onClose={reset}>{error}</Alert>}

      {/* ── Tabla de vuelos ── */}
      {loading || airLoading ? (
        <div className="text-center py-5"><Spinner animation="border" /></div>
      ) : (
        <div className="bg-white rounded shadow-sm overflow-hidden">
          <Table responsive hover className="mb-0">
            <thead className="table-light">
              <tr>
                <th>#</th>
                <th>Origen</th>
                <th>Destino</th>
                <th>Salida</th>
                <th>Llegada</th>
                <th>Avión</th>
                <th>Precio</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {flights.map(f => {
                const flightNumber = f.flightNumber ?? String(f.num_Vuelo);
                const isBoarding = f.estado === 'Boarding';
                const canClose = isBoarding && withinOneHour(f.hora_Salida);

                return (
                  <tr key={f.num_Vuelo}>
                    <td className="fw-bold">{flightNumber}</td>
                    <td className="small">{airportName(f.id_Aeropuerto_Origen)}</td>
                    <td className="small">{airportName(f.id_Aeropuerto_Destino)}</td>
                    <td className="small">
                      {new Date(f.hora_Salida).toLocaleString('es-CR')}
                      {isBoarding && (
                        <div className="text-muted" style={{ fontSize: '0.7rem' }}>
                          Sale en: {timeUntilDeparture(f.hora_Salida)}
                        </div>
                      )}
                    </td>
                    <td className="small">{new Date(f.hora_Llegada).toLocaleString('es-CR')}</td>
                    <td>{f.matricula}</td>
                    <td className="fw-medium">${(f.precio ?? 0).toFixed(2)}</td>
                    <td>
                      <Badge bg={STATUS_BADGE[f.estado]}>
                        {STATUS_LABELS[f.estado]}
                      </Badge>
                    </td>
                    <td>
                      <div className="d-flex gap-2 flex-wrap">
                        {/* Botón Cerrar Vuelo — solo aparece en estado Boarding */}
                        {isBoarding && (
                          <Button
                            variant={canClose ? 'danger' : 'outline-danger'}
                            size="sm"
                            onClick={() => openCloseModal(f)}
                            title={
                              canClose
                                ? 'Cerrar vuelo y generar lista oficial'
                                : 'El cierre solo está disponible dentro de 1 hora antes de la salida'
                            }
                          >
                            <i className="bi bi-door-closed me-1" />
                            Cerrar Vuelo
                          </Button>
                        )}
                        <Button
                          variant="outline-dark"
                          size="sm"
                          onClick={() => openEdit(f)}
                        >
                          <i className="bi bi-pencil" />
                        </Button>
                        <Button
                          variant="outline-danger"
                          size="sm"
                          disabled={deleting === flightNumber}
                          onClick={() => handleDelete(flightNumber)}
                        >
                          <i className="bi bi-trash" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </Table>
        </div>
      )}

      {/* ════════════════════════════════════════════════════════════════════
          MODAL — CIERRE DE VUELO
          Muestra la lista oficial de pasajeros con check-in y permite
          confirmar el cierre según el enunciado.
          ════════════════════════════════════════════════════════════════════ */}
      <Modal
        show={closeModal}
        onHide={handleCloseModalDismiss}
        centered
        size="lg"
      >
        <Modal.Header closeButton className={closeSuccess ? 'bg-success bg-opacity-10' : ''}>
          <Modal.Title className="fw-bold">
            {closeSuccess
              ? '✓ Vuelo cerrado exitosamente'
              : `Cierre de Vuelo — ${closingFlight?.flightNumber ?? ''}`}
          </Modal.Title>
        </Modal.Header>

        <Modal.Body>
          {/* Error de carga o de cierre */}
          {closeError && (
            <Alert variant="danger" className="mb-3">
              <i className="bi bi-exclamation-triangle me-2" />
              {closeError}
            </Alert>
          )}

          {/* Éxito */}
          {closeSuccess && (
            <Alert variant="success" className="mb-3">
              <i className="bi bi-check-circle me-2" />
              El vuelo <strong>{closingFlight?.flightNumber}</strong> ha sido cerrado.
              La lista oficial de pasajeros está disponible a continuación.
            </Alert>
          )}

          {/* Advertencia de tiempo — si aún no está dentro de la hora */}
          {closingFlight && !withinOneHour(closingFlight.hora_Salida) && !closeSuccess && (
            <Alert variant="warning" className="mb-3">
              <i className="bi bi-clock me-2" />
              <strong>Atención:</strong>  El cierre debe realizarse{' '}
              <strong>una hora antes de la salida</strong>. Este vuelo sale en{' '}
              <strong>{timeUntilDeparture(closingFlight.hora_Salida)}</strong>.
              Puede consultar la lista ahora, pero el cierre oficial debería hacerse
              más cerca de la hora de salida.
            </Alert>
          )}

          {/* Spinner mientras carga la lista */}
          {loadingFinalList && (
            <div className="text-center py-4">
              <Spinner animation="border" />
              <p className="text-muted mt-2 mb-0">Cargando lista de pasajeros...</p>
            </div>
          )}

          {/* Lista oficial de pasajeros */}
          {finalList && !loadingFinalList && (
            <>
              {/* Resumen del vuelo */}
              <Row className="g-3 mb-4">
                <Col xs={6} md={3}>
                  <Card className="border-0 bg-light text-center py-2">
                    <div className="fs-4 fw-bold text-dark">{finalList.totalPassengers}</div>
                    <div className="small text-muted">Pasajeros</div>
                  </Card>
                </Col>
                <Col xs={6} md={3}>
                  <Card className="border-0 bg-light text-center py-2">
                    <div className="fs-4 fw-bold text-dark">{finalList.totalBaggages}</div>
                    <div className="small text-muted">Maletas</div>
                  </Card>
                </Col>
                <Col xs={6} md={3}>
                  <Card className="border-0 bg-light text-center py-2">
                    <div className="fs-6 fw-bold text-dark">
                      {new Date(finalList.departureTime).toLocaleTimeString('es-CR', {
                        hour: '2-digit', minute: '2-digit',
                      })}
                    </div>
                    <div className="small text-muted">Salida</div>
                  </Card>
                </Col>
                <Col xs={6} md={3}>
                  <Card className="border-0 bg-light text-center py-2">
                    <div className="fs-6 fw-bold text-dark">
                      {new Date(finalList.arrivalTime).toLocaleTimeString('es-CR', {
                        hour: '2-digit', minute: '2-digit',
                      })}
                    </div>
                    <div className="small text-muted">Llegada</div>
                  </Card>
                </Col>
              </Row>

              <h6 className="fw-bold mb-2">
                Lista oficial de pasajeros con check-in
              </h6>
              <p className="text-muted small mb-3">
                {closeSuccess
                  ? 'Esta es la lista definitiva de pasajeros que viajan en este vuelo.'
                  : 'Solo los pasajeros que realizaron check-in aparecen en esta lista.'}
              </p>

              {finalList.passengers.length === 0 ? (
                <Alert variant="info">
                  <i className="bi bi-info-circle me-2" />
                  No hay pasajeros con check-in registrado para este vuelo.
                </Alert>
              ) : (
                <div className="table-responsive">
                  <Table size="sm" bordered hover className="mb-0">
                    <thead className="table-dark">
                      <tr>
                        <th>Pasajero</th>
                        <th>Asiento</th>
                        <th>Puerta</th>
                        <th className="text-center">Maletas</th>
                        <th className="text-end">Cargo extra</th>
                      </tr>
                    </thead>
                    <tbody>
                      {finalList.passengers.map(p => (
                        <tr key={p.checkInId}>
                          <td>
                            <div className="fw-medium">{p.fullName}</div>
                            <div className="text-muted small">{p.email}</div>
                          </td>
                          <td>
                            <Badge bg="secondary">{p.seat}</Badge>
                          </td>
                          <td>{p.boardingGate}</td>
                          <td className="text-center">{p.baggageCount}</td>
                          <td className="text-end">
                            {p.baggageSurcharge > 0
                              ? <span className="text-warning fw-medium">${p.baggageSurcharge}</span>
                              : <span className="text-success">Gratis</span>}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot className="table-light">
                      <tr>
                        <td colSpan={3} className="fw-bold">Total</td>
                        <td className="text-center fw-bold">{finalList.totalBaggages}</td>
                        <td className="text-end fw-bold">
                          ${finalList.passengers.reduce((s, p) => s + p.baggageSurcharge, 0).toFixed(2)}
                        </td>
                      </tr>
                    </tfoot>
                  </Table>
                </div>
              )}
            </>
          )}
        </Modal.Body>

        <Modal.Footer>
          <Button variant="outline-secondary" onClick={handleCloseModalDismiss}>
            {closeSuccess ? 'Cerrar' : 'Cancelar'}
          </Button>
          {/* Botón confirmar cierre — oculto si ya se cerró exitosamente */}
          {!closeSuccess && (
            <Button
              variant="danger"
              disabled={loadingFinalList || confirmingClose || !!closeError}
              onClick={handleConfirmClose}
            >
              {confirmingClose
                ? <><Spinner size="sm" animation="border" className="me-2" />Cerrando...</>
                : <><i className="bi bi-door-closed me-2" />Confirmar Cierre</>}
            </Button>
          )}
        </Modal.Footer>
      </Modal>

      {/* ════════════════════════════════════════════════════════════════════
          MODAL — CRUD (Crear / Editar vuelo)
          ════════════════════════════════════════════════════════════════════ */}
      <Modal show={showModal} onHide={() => setShowModal(false)} centered size="lg">
        <Modal.Header closeButton>
          <Modal.Title className="fw-bold">
            {editing ? 'Editar Vuelo' : 'Nuevo Vuelo'}
          </Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSave}>
          <Modal.Body>
            {!editing && (
              <Form.Group className="mb-3">
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Número de Vuelo
                </Form.Label>
                <Form.Control
                  className="minimal-input"
                  placeholder="Ej: TA-210"
                  value={form.flightNumber ?? ''}
                  onChange={set('flightNumber')}
                  required={!editing}
                />
              </Form.Group>
            )}
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Aeropuerto Origen
                </Form.Label>
                <Form.Select
                  value={form.id_Aeropuerto_Origen}
                  onChange={set('id_Aeropuerto_Origen')}
                  required
                >
                  <option value={0}>Selecciona...</option>
                  {airports.map(a => (
                    <option key={a.airportId} value={a.airportId}>{a.name}</option>
                  ))}
                </Form.Select>
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Aeropuerto Destino
                </Form.Label>
                <Form.Select
                  value={form.id_Aeropuerto_Destino}
                  onChange={set('id_Aeropuerto_Destino')}
                  required
                >
                  <option value={0}>Selecciona...</option>
                  {airports
                    .filter(a => a.airportId !== form.id_Aeropuerto_Origen)
                    .map(a => (
                      <option key={a.airportId} value={a.airportId}>{a.name}</option>
                    ))}
                </Form.Select>
              </Form.Group>
            </Row>
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Hora Salida
                </Form.Label>
                <Form.Control
                  type="datetime-local"
                  className="minimal-input"
                  value={form.hora_Salida}
                  onChange={set('hora_Salida')}
                  required
                />
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Hora Llegada
                </Form.Label>
                <Form.Control
                  type="datetime-local"
                  className="minimal-input"
                  value={form.hora_Llegada}
                  onChange={set('hora_Llegada')}
                  required
                />
              </Form.Group>
            </Row>
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Precio (USD)
                </Form.Label>
                <Form.Control
                  type="number"
                  min={0}
                  step="0.01"
                  className="minimal-input"
                  placeholder="0.00"
                  value={form.precio ?? 0}
                  onChange={set('precio')}
                  required
                />
              </Form.Group>
            </Row>
            <Row>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Avión (Matrícula)
                </Form.Label>
                <Form.Select
                  value={form.matricula}
                  onChange={set('matricula')}
                  required
                  disabled={planeLoading}
                >
                  <option value="">
                    {planeLoading ? 'Cargando aviones...' : 'Selecciona...'}
                  </option>
                  {airplanes.map(p => (
                    <option key={p.plateNumber} value={p.plateNumber}>
                      {p.plateNumber} ({p.passengerCapacity} pasajeros)
                    </option>
                  ))}
                </Form.Select>
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">
                  Estado
                </Form.Label>
                <Form.Select value={form.estado} onChange={set('estado')}>
                  {STATUS_OPTIONS.map(s => (
                    <option key={s} value={s}>{STATUS_LABELS[s]}</option>
                  ))}
                </Form.Select>
              </Form.Group>
            </Row>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="outline-secondary" onClick={() => setShowModal(false)}>
              Cancelar
            </Button>
            <Button variant="dark" type="submit" disabled={saving}>
              {saving ? 'Guardando...' : 'Guardar'}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </>
  );
};

export default FlightManagementView;