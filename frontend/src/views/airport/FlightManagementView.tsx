import { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Alert, Spinner, Row, Col } from 'react-bootstrap';
import { flightService } from '../../services/flightService';
import { airportService } from '../../services/airportService';
import type { Flight, FlightCreate, FlightStatus, Airport } from '../../types';

const STATUS_LABELS: Record<FlightStatus, string> = {
  programado: 'Programado',
  abierto:    'Abordaje',
  cerrado:    'Aterrizado',
  cancelado:  'Cancelado',
};

const STATUS_OPTIONS: FlightStatus[] = ['programado', 'abierto', 'cerrado', 'cancelado'];

const EMPTY_FORM: FlightCreate = {
  flightNumber: '', hora_Salida: '', hora_Llegada: '', estado: 'programado',
  matricula: '', id_Aeropuerto_Origen: 0, id_Aeropuerto_Destino: 0,
};

const FlightManagementView = () => {
  const [flights, setFlights] = useState<Flight[]>([]);
  const [airports, setAirports] = useState<Airport[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Flight | null>(null);
  const [form, setForm] = useState<FlightCreate>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState<string | null>(null);
  const [changingStatus, setChangingStatus] = useState<string | null>(null);

  const load = () => {
    setLoading(true);
    Promise.all([flightService.getAll(), airportService.getAll()])
      .then(([f, a]) => { setFlights(f); setAirports(a); })
      .catch(() => setError('No se pudieron cargar los datos.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const airportName = (id: number) => airports.find(a => a.id_Aeropuerto === id)?.nombre ?? `#${id}`;

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
    });
    setShowModal(true);
  };

  const handleSave = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editing) {
        await flightService.update(editing.flightNumber ?? String(editing.num_Vuelo), form);
      } else {
        await flightService.create(form);
      }
      setShowModal(false);
      load();
    } catch {
      setError('No se pudo guardar el vuelo.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (flightNumber: string) => {
    if (!confirm('¿Eliminar este vuelo?')) return;
    setDeleting(flightNumber);
    try {
      await flightService.remove(flightNumber);
      load();
    } catch {
      setError('No se pudo eliminar el vuelo.');
    } finally {
      setDeleting(null);
    }
  };

  const handleStatusChange = async (flightNumber: string, newStatus: FlightStatus) => {
    setChangingStatus(flightNumber);
    try {
      await flightService.updateStatus(flightNumber, newStatus);
      setFlights(prev => prev.map(f =>
        (f.flightNumber ?? String(f.num_Vuelo)) === flightNumber ? { ...f, estado: newStatus } : f
      ));
    } catch {
      setError('No se pudo cambiar el estado del vuelo.');
    } finally {
      setChangingStatus(null);
    }
  };

  const set = (field: keyof FlightCreate) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
    setForm(prev => ({ ...prev, [field]: field.startsWith('id_') ? Number(e.target.value) : e.target.value }));

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 className="fw-bold mb-1">Vuelos</h3>
          <p className="text-muted mb-0">Administración de vuelos programados.</p>
        </div>
        <Button variant="dark" onClick={openCreate}>
          <i className="bi bi-plus-lg me-2"></i>Nuevo Vuelo
        </Button>
      </div>

      {error && <Alert variant="danger" dismissible onClose={() => setError('')}>{error}</Alert>}

      {loading ? (
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
                <th>Estado</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {flights.map(f => (
                <tr key={f.num_Vuelo}>
                  <td className="fw-bold">{f.num_Vuelo}</td>
                  <td className="small">{airportName(f.id_Aeropuerto_Origen)}</td>
                  <td className="small">{airportName(f.id_Aeropuerto_Destino)}</td>
                  <td className="small">{new Date(f.hora_Salida).toLocaleString('es-CR')}</td>
                  <td className="small">{new Date(f.hora_Llegada).toLocaleString('es-CR')}</td>
                  <td>{f.matricula}</td>
                  <td>
                    <Form.Select
                      size="sm"
                      value={f.estado}
                      disabled={changingStatus === (f.flightNumber ?? String(f.num_Vuelo))}
                      style={{ minWidth: '130px' }}
                      onChange={e => handleStatusChange(
                        f.flightNumber ?? String(f.num_Vuelo),
                        e.target.value as FlightStatus
                      )}
                    >
                      {STATUS_OPTIONS.map(s => (
                        <option key={s} value={s}>{STATUS_LABELS[s]}</option>
                      ))}
                    </Form.Select>
                  </td>
                  <td>
                    <Button variant="outline-dark" size="sm" className="me-2" onClick={() => openEdit(f)}>
                      <i className="bi bi-pencil"></i>
                    </Button>
                    <Button variant="outline-danger" size="sm" disabled={deleting === (f.flightNumber ?? String(f.num_Vuelo))} onClick={() => handleDelete(f.flightNumber ?? String(f.num_Vuelo))}>
                      <i className="bi bi-trash"></i>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      )}

      <Modal show={showModal} onHide={() => setShowModal(false)} centered size="lg">
        <Modal.Header closeButton>
          <Modal.Title className="fw-bold">{editing ? 'Editar Vuelo' : 'Nuevo Vuelo'}</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSave}>
          <Modal.Body>
            {!editing && (
              <Form.Group className="mb-3">
                <Form.Label className="text-muted small text-uppercase fw-bold">Número de Vuelo</Form.Label>
                <Form.Control className="minimal-input" placeholder="Ej: TA-210" value={form.flightNumber ?? ''} onChange={set('flightNumber')} required={!editing} />
              </Form.Group>
            )}
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Aeropuerto Origen</Form.Label>
                <Form.Select value={form.id_Aeropuerto_Origen} onChange={set('id_Aeropuerto_Origen')} required>
                  <option value={0}>Selecciona...</option>
                  {airports.map(a => <option key={a.id_Aeropuerto} value={a.id_Aeropuerto}>{a.nombre}</option>)}
                </Form.Select>
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Aeropuerto Destino</Form.Label>
                <Form.Select value={form.id_Aeropuerto_Destino} onChange={set('id_Aeropuerto_Destino')} required>
                  <option value={0}>Selecciona...</option>
                  {airports.filter(a => a.id_Aeropuerto !== form.id_Aeropuerto_Origen).map(a =>
                    <option key={a.id_Aeropuerto} value={a.id_Aeropuerto}>{a.nombre}</option>
                  )}
                </Form.Select>
              </Form.Group>
            </Row>
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Hora Salida</Form.Label>
                <Form.Control type="datetime-local" className="minimal-input" value={form.hora_Salida} onChange={set('hora_Salida')} required />
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Hora Llegada</Form.Label>
                <Form.Control type="datetime-local" className="minimal-input" value={form.hora_Llegada} onChange={set('hora_Llegada')} required />
              </Form.Group>
            </Row>
            <Row>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Matrícula Avión</Form.Label>
                <Form.Control className="minimal-input" placeholder="Ej: TEC-001" value={form.matricula} onChange={set('matricula')} required />
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Estado</Form.Label>
                <Form.Select value={form.estado} onChange={set('estado')}>
                  {STATUS_OPTIONS.map(s => <option key={s} value={s}>{s}</option>)}
                </Form.Select>
              </Form.Group>
            </Row>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="outline-secondary" onClick={() => setShowModal(false)}>Cancelar</Button>
            <Button variant="dark" type="submit" disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </>
  );
};

export default FlightManagementView;
