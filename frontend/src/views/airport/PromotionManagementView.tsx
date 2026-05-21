import { useState, useEffect } from 'react';
import { Table, Badge, Button, Modal, Form, Alert, Spinner, Row, Col } from 'react-bootstrap';
import { promotionService } from '../../services/promotionService';
import { airportService } from '../../services/airportService';
import type { Promotion, PromotionCreate, Airport } from '../../types';

const EMPTY_FORM: PromotionCreate = {
  precio: 0, fecha_Inicio: '', fecha_Fin: '', imagen: '',
  estado_Activa: true, id_Aeropuerto_Origen: 0, id_Aeropuerto_Destino: 0,
};

const PromotionManagementView = () => {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [airports, setAirports] = useState<Airport[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Promotion | null>(null);
  const [form, setForm] = useState<PromotionCreate>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState<number | null>(null);

  const load = () => {
    setLoading(true);
    Promise.all([promotionService.getAll(), airportService.getAll()])
      .then(([p, a]) => { setPromotions(p); setAirports(a); })
      .catch(() => setError('No se pudieron cargar los datos.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const airportName = (id: number) => airports.find(a => a.id_Aeropuerto === id)?.nombre ?? `#${id}`;

  const openCreate = () => { setEditing(null); setForm(EMPTY_FORM); setShowModal(true); };
  const openEdit = (p: Promotion) => {
    setEditing(p);
    setForm({
      precio: p.precio, fecha_Inicio: p.fecha_Inicio, fecha_Fin: p.fecha_Fin,
      imagen: p.imagen ?? '', estado_Activa: p.estado_Activa,
      id_Aeropuerto_Origen: p.id_Aeropuerto_Origen, id_Aeropuerto_Destino: p.id_Aeropuerto_Destino,
    });
    setShowModal(true);
  };

  const handleSave = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editing) {
        await promotionService.update(editing.id_Promocion, form);
      } else {
        await promotionService.create(form);
      }
      setShowModal(false);
      load();
    } catch {
      setError('No se pudo guardar la promoción.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('¿Eliminar esta promoción?')) return;
    setDeleting(id);
    try {
      await promotionService.remove(id);
      load();
    } catch {
      setError('No se pudo eliminar la promoción.');
    } finally {
      setDeleting(null);
    }
  };

  const set = (field: keyof PromotionCreate) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const val = field.startsWith('id_') ? Number(e.target.value)
      : field === 'precio' ? Number(e.target.value)
      : field === 'estado_Activa' ? (e.target as HTMLInputElement).checked
      : e.target.value;
    setForm(prev => ({ ...prev, [field]: val }));
  };

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 className="fw-bold mb-1">Promociones</h3>
          <p className="text-muted mb-0">Gestión de ofertas especiales de TECAir.</p>
        </div>
        <Button variant="dark" onClick={openCreate}>
          <i className="bi bi-plus-lg me-2"></i>Nueva Promoción
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
                <th>Precio</th>
                <th>Vigencia</th>
                <th>Estado</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {promotions.map(p => (
                <tr key={p.id_Promocion}>
                  <td>{p.id_Promocion}</td>
                  <td className="small">{airportName(p.id_Aeropuerto_Origen)}</td>
                  <td className="small">{airportName(p.id_Aeropuerto_Destino)}</td>
                  <td className="fw-bold">${p.precio.toFixed(2)}</td>
                  <td className="small text-muted">
                    {new Date(p.fecha_Inicio).toLocaleDateString('es-CR')} — {new Date(p.fecha_Fin).toLocaleDateString('es-CR')}
                  </td>
                  <td><Badge bg={p.estado_Activa ? 'success' : 'secondary'}>{p.estado_Activa ? 'Activa' : 'Inactiva'}</Badge></td>
                  <td>
                    <Button variant="outline-dark" size="sm" className="me-2" onClick={() => openEdit(p)}>
                      <i className="bi bi-pencil"></i>
                    </Button>
                    <Button variant="outline-danger" size="sm" disabled={deleting === p.id_Promocion} onClick={() => handleDelete(p.id_Promocion)}>
                      <i className="bi bi-trash"></i>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      )}

      <Modal show={showModal} onHide={() => setShowModal(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title className="fw-bold">{editing ? 'Editar Promoción' : 'Nueva Promoción'}</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSave}>
          <Modal.Body>
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Origen</Form.Label>
                <Form.Select value={form.id_Aeropuerto_Origen} onChange={set('id_Aeropuerto_Origen')} required>
                  <option value={0}>Selecciona...</option>
                  {airports.map(a => <option key={a.id_Aeropuerto} value={a.id_Aeropuerto}>{a.nombre}</option>)}
                </Form.Select>
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Destino</Form.Label>
                <Form.Select value={form.id_Aeropuerto_Destino} onChange={set('id_Aeropuerto_Destino')} required>
                  <option value={0}>Selecciona...</option>
                  {airports.filter(a => a.id_Aeropuerto !== form.id_Aeropuerto_Origen).map(a =>
                    <option key={a.id_Aeropuerto} value={a.id_Aeropuerto}>{a.nombre}</option>
                  )}
                </Form.Select>
              </Form.Group>
            </Row>
            <Form.Group className="mb-3">
              <Form.Label className="text-muted small text-uppercase fw-bold">Precio ($)</Form.Label>
              <Form.Control type="number" step="0.01" min="0.01" className="minimal-input" value={form.precio} onChange={set('precio')} required />
            </Form.Group>
            <Row className="mb-3">
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Fecha Inicio</Form.Label>
                <Form.Control type="date" className="minimal-input" value={form.fecha_Inicio} onChange={set('fecha_Inicio')} required />
              </Form.Group>
              <Form.Group as={Col} md={6}>
                <Form.Label className="text-muted small text-uppercase fw-bold">Fecha Fin</Form.Label>
                <Form.Control type="date" className="minimal-input" value={form.fecha_Fin} onChange={set('fecha_Fin')} required />
              </Form.Group>
            </Row>
            <Form.Group className="mb-3">
              <Form.Label className="text-muted small text-uppercase fw-bold">URL Imagen (opcional)</Form.Label>
              <Form.Control className="minimal-input" placeholder="https://..." value={form.imagen ?? ''} onChange={set('imagen')} />
            </Form.Group>
            <Form.Check
              type="switch"
              label="Promoción activa"
              checked={form.estado_Activa}
              onChange={e => setForm(prev => ({ ...prev, estado_Activa: e.target.checked }))}
            />
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

export default PromotionManagementView;
