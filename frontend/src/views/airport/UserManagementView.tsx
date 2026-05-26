import { useState, useEffect } from 'react';
import { Table, Badge, Button, Modal, Form, Alert, Spinner } from 'react-bootstrap';
import { userService } from '../../services/userService';
import type { User, UserRequest, UserRole } from '../../types';

const ROLE_BADGE: Record<UserRole, string> = {
  cliente:        'primary',
  funcionario:    'info',
  administrador:  'danger',
};

const EMPTY_FORM: UserRequest = {
  nombre: '', correo: '', telefono: '', rol: 'cliente', millas: 0, carnet: '', universidad: '',
};

const UserManagementView = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<User | null>(null);
  const [form, setForm] = useState<UserRequest>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState<number | null>(null);

  const load = () => {
    setLoading(true);
    userService.getAll()
      .then(setUsers)
      .catch(() => setError('No se pudieron cargar los usuarios.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const filtered = users.filter(u =>
    u.nombre.toLowerCase().includes(search.toLowerCase()) ||
    u.correo.toLowerCase().includes(search.toLowerCase())
  );

  const openEdit = (u: User) => {
    setEditing(u);
    setForm({ nombre: u.nombre, correo: u.correo, telefono: u.telefono, rol: u.rol, millas: u.millas, carnet: u.carnet ?? '', universidad: u.universidad ?? '' });
  };

  const handleSave = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    if (!editing) return;
    setSaving(true);
    try {
      await userService.update(editing.id_Usuario, form);
      setEditing(null);
      load();
    } catch {
      setError('No se pudo actualizar el usuario.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('¿Eliminar este usuario permanentemente?')) return;
    setDeleting(id);
    try {
      await userService.remove(id);
      load();
    } catch {
      setError('No se pudo eliminar el usuario.');
    } finally {
      setDeleting(null);
    }
  };

  const set = (field: keyof UserRequest) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const value = field === 'millas' ? Number(e.target.value) : e.target.value;
    setForm(prev => ({ ...prev, [field]: value }));
  };

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h3 className="fw-bold mb-1">Usuarios</h3>
          <p className="text-muted mb-0">Gestión de cuentas registradas en el sistema.</p>
        </div>
      </div>

      {error && <Alert variant="danger" dismissible onClose={() => setError('')}>{error}</Alert>}

      <div className="bg-white rounded shadow-sm p-3 mb-3">
        <Form.Control
          type="search"
          placeholder="Buscar por nombre o correo..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          className="minimal-input"
        />
      </div>

      {loading ? (
        <div className="text-center py-5"><Spinner animation="border" /></div>
      ) : (
        <div className="bg-white rounded shadow-sm overflow-hidden">
          <Table responsive hover className="mb-0">
            <thead className="table-light">
              <tr>
                <th>#</th>
                <th>Nombre</th>
                <th>Correo</th>
                <th>Teléfono</th>
                <th>Rol</th>
                <th>Millas</th>
                <th>Universidad</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(u => (
                <tr key={u.id_Usuario}>
                  <td>{u.id_Usuario}</td>
                  <td className="fw-medium">{u.nombre}</td>
                  <td className="text-muted small">{u.correo}</td>
                  <td>{u.telefono}</td>
                  <td><Badge bg={ROLE_BADGE[u.rol]}>{u.rol}</Badge></td>
                  <td>{u.millas}</td>
                  <td className="text-muted small">{u.universidad ?? '—'}</td>
                  <td>
                    <Button variant="outline-dark" size="sm" className="me-2" onClick={() => openEdit(u)}>
                      <i className="bi bi-pencil"></i>
                    </Button>
                    <Button variant="outline-danger" size="sm" disabled={deleting === u.id_Usuario} onClick={() => handleDelete(u.id_Usuario)}>
                      <i className="bi bi-trash"></i>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      )}

      {/* Edit Modal */}
      <Modal show={!!editing} onHide={() => setEditing(null)} centered>
        <Modal.Header closeButton>
          <Modal.Title className="fw-bold">Editar Usuario</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSave}>
          <Modal.Body>
            {(['nombre', 'correo', 'telefono'] as const).map(field => (
              <Form.Group key={field} className="mb-3">
                <Form.Label className="text-muted small text-uppercase fw-bold">{field}</Form.Label>
                <Form.Control
                  className="minimal-input"
                  value={String(form[field] ?? '')}
                  onChange={set(field)}
                  required
                />
              </Form.Group>
            ))}
            <Form.Group className="mb-3">
              <Form.Label className="text-muted small text-uppercase fw-bold">Rol</Form.Label>
              <Form.Select value={form.rol} onChange={set('rol')}>
                <option value="cliente">Cliente</option>
                <option value="funcionario">Funcionario</option>
                <option value="administrador">Administrador</option>
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label className="text-muted small text-uppercase fw-bold">Millas</Form.Label>
              <Form.Control type="number" className="minimal-input" value={form.millas ?? 0} onChange={set('millas')} min={0} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label className="text-muted small text-uppercase fw-bold">Universidad</Form.Label>
              <Form.Control className="minimal-input" value={form.universidad ?? ''} onChange={set('universidad')} />
            </Form.Group>
            <Form.Group className="mb-0">
              <Form.Label className="text-muted small text-uppercase fw-bold">Carnet</Form.Label>
              <Form.Control className="minimal-input" value={form.carnet ?? ''} onChange={set('carnet')} />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="outline-secondary" onClick={() => setEditing(null)}>Cancelar</Button>
            <Button variant="dark" type="submit" disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </>
  );
};

export default UserManagementView;
