import { useState, useEffect } from 'react';
import { Row, Col, Form, Button, Alert, Card, Table, Spinner } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import { useCheckIn } from '../../hooks/useCheckIn';
import { useBaggage } from '../../hooks/useBaggage';

const CheckInManagementView = () => {
  const [searchParams] = useSearchParams();
  const initialCod = searchParams.get('cod') ?? '';
  const autoMode   = searchParams.get('auto') === 'true';

  const {
    boardingPass, loading: checkInLoading, error: checkInError,
    executeCheckIn, resetCheckIn,
  } = useCheckIn();

  const {
    baggages, loading: bagLoading, error: bagError,
    getBaggageByReservation, addBaggage, reset: resetBaggage,
  } = useBaggage();

  const [codReservacion, setCodReservacion] = useState(initialCod);
  const [bagForm, setBagForm] = useState<{ peso: string; color: string }>({ peso: '', color: '' });

  const showAutoSpinner = autoMode && (checkInLoading || (!boardingPass && !checkInError));

  useEffect(() => {
    if (!autoMode || !initialCod) return;
    void (async () => {
      const pass = await executeCheckIn(initialCod.trim(), '');
      if (pass) {
        await getBaggageByReservation(initialCod.trim());
      }
    })();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCheckIn = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    resetBaggage();
    const pass = await executeCheckIn(codReservacion.trim(), '');
    if (pass) {
      await getBaggageByReservation(codReservacion.trim());
    }
  };

  const handleAddBag = async (e: { preventDefault(): void }) => {
    e.preventDefault();
    if (!boardingPass) return;
    const bag = await addBaggage({
      peso: Number(bagForm.peso),
      color: bagForm.color,
      cod_Reservacion: boardingPass.cod_Reservacion,
      id_Usuario: 0,
    });
    if (bag) setBagForm({ peso: '', color: '' });
  };

  const baggageFee = () => {
    if (baggages.length <= 1) return 0;
    if (baggages.length === 2) return 50;
    return 50 + (baggages.length - 2) * 75;
  };

  const reset = () => {
    resetCheckIn();
    resetBaggage();
    setCodReservacion('');
  };

  return (
    <>
      <h3 className="fw-bold mb-1">Check-in</h3>
      <p className="text-muted mb-4">Gestión de chequeo de pasajeros en el aeropuerto.</p>

      {showAutoSpinner ? (
        <div className="text-center py-5">
          <Spinner animation="border" className="mb-3" />
          <p className="text-muted">Realizando check-in...</p>
        </div>
      ) : !boardingPass ? (
        <Row className="justify-content-center">
          <Col md={6}>
            <div className="bg-white rounded shadow-sm p-4">
              <h5 className="fw-bold mb-4">Buscar Reservación</h5>
              {checkInError && <Alert variant="danger">{checkInError}</Alert>}
              <Form onSubmit={handleCheckIn}>
                <Form.Group className="mb-4">
                  <Form.Label className="text-muted small text-uppercase fw-bold">Código de Reservación</Form.Label>
                  <Form.Control
                    type="text"
                    className="minimal-input"
                    placeholder="Ej: RES-001"
                    value={codReservacion}
                    onChange={e => setCodReservacion(e.target.value)}
                    required
                  />
                </Form.Group>
                <Button variant="dark" type="submit" className="w-100" disabled={checkInLoading}>
                  {checkInLoading ? 'Procesando...' : 'Realizar Check-in'}
                </Button>
              </Form>
            </div>
          </Col>
        </Row>
      ) : (
        <Row className="g-4">
          <Col md={5}>
            <Card className="border-0 shadow-sm">
              <Card.Body className="p-4">
                <div className="d-flex justify-content-between align-items-start mb-3">
                  <h5 className="fw-bold mb-0">Pase de Abordar</h5>
                  <span className="badge bg-success">Check-in OK</span>
                </div>
                {[
                  ['Vuelo',       `#${boardingPass.num_Vuelo}`],
                  ['Reservación', `#${boardingPass.cod_Reservacion}`],
                  ['Asiento',     boardingPass.asiento],
                  ['Puerta',      boardingPass.puerta_Abordaje],
                  ['Impreso',     new Date(boardingPass.hora_Impresion).toLocaleString('es-CR')],
                ].map(([label, val]) => (
                  <div key={label} className="d-flex justify-content-between mb-2">
                    <span className="text-muted">{label}</span>
                    <span className="fw-bold">{val}</span>
                  </div>
                ))}
                <Button variant="outline-secondary" size="sm" className="w-100 mt-3" onClick={reset}>
                  Nuevo Check-in
                </Button>
              </Card.Body>
            </Card>
          </Col>

          <Col md={7}>
            <Card className="border-0 shadow-sm">
              <Card.Body className="p-4">
                <h5 className="fw-bold mb-3">Maletas</h5>
                {bagError && <Alert variant="danger" className="py-2 small">{bagError}</Alert>}
                {bagLoading && baggages.length === 0 ? (
                  <div className="text-center py-3"><Spinner animation="border" size="sm" /></div>
                ) : baggages.length > 0 && (
                  <Table size="sm" className="mb-3">
                    <thead><tr><th>#</th><th>Color</th><th>Peso (kg)</th></tr></thead>
                    <tbody>
                      {baggages.map(b => (
                        <tr key={b.num_Maleta}>
                          <td>{b.num_Maleta}</td>
                          <td>{b.color}</td>
                          <td>{b.peso}</td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                )}
                {baggages.length > 0 && (
                  <Alert variant={baggageFee() > 0 ? 'warning' : 'success'} className="py-2 small">
                    {baggages.length === 1
                      ? '1ra maleta: gratis'
                      : `Cargo adicional: $${baggageFee()} (${baggages.length} maletas)`}
                  </Alert>
                )}
                <Form onSubmit={handleAddBag}>
                  <Row className="g-2 align-items-end">
                    <Col>
                      <Form.Label className="text-muted small text-uppercase fw-bold">Color</Form.Label>
                      <Form.Control
                        size="sm"
                        className="minimal-input"
                        placeholder="Ej: Negro"
                        value={bagForm.color}
                        onChange={e => setBagForm(p => ({ ...p, color: e.target.value }))}
                        required
                      />
                    </Col>
                    <Col>
                      <Form.Label className="text-muted small text-uppercase fw-bold">Peso (kg)</Form.Label>
                      <Form.Control
                        size="sm"
                        type="number"
                        step="0.1"
                        min="0.1"
                        className="minimal-input"
                        placeholder="23.0"
                        value={bagForm.peso}
                        onChange={e => setBagForm(p => ({ ...p, peso: e.target.value }))}
                        required
                      />
                    </Col>
                    <Col xs="auto">
                      <Button variant="dark" size="sm" type="submit" disabled={bagLoading}>
                        {bagLoading ? '...' : 'Agregar'}
                      </Button>
                    </Col>
                  </Row>
                </Form>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}
    </>
  );
};

export default CheckInManagementView;
