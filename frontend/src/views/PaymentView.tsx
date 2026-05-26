import { useState } from 'react';
import { Container, Form, Button, Alert, Card, Row, Col } from 'react-bootstrap';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { paymentService } from '../services/paymentService';

const PaymentView = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const reservationCode = searchParams.get('cod') ?? '';
  const montoParam = searchParams.get('monto');

  const [form, setForm] = useState({
    amount: montoParam ?? '',
    cardNumber: '',
    cardholderName: '',
    expirationDate: '',
    cvv: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const set = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await paymentService.process({
        reservationCode,
        amount: Number(form.amount),
        cardNumber: form.cardNumber,
        cardholderName: form.cardholderName,
        expirationDate: form.expirationDate,
        cvv: form.cvv,
      });
      setSuccess(true);
      setTimeout(() => navigate('/mis-reservaciones'), 2500);
    } catch (err: any) {
      const data = err.response?.data;
      let msg = 'No se pudo procesar el pago. Verifica los datos de tu tarjeta.';
      if (data) {
        if (data.errors) {
          const first = Object.values(data.errors as Record<string, string[]>)[0];
          if (first?.length) msg = first[0];
        } else {
          msg = data.message || data.Message || data.title || msg;
        }
      }
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <Container className="py-5 d-flex justify-content-center">
        <div className="text-center" style={{ maxWidth: '450px' }}>
          <div className="display-1 mb-3">✓</div>
          <h3 className="fw-bold mb-2">¡Pago exitoso!</h3>
          <p className="text-muted">Tu reservación <strong>{reservationCode}</strong> ha sido confirmada y pagada.</p>
          <p className="text-muted small">Redirigiendo a tus reservaciones...</p>
        </div>
      </Container>
    );
  }

  return (
    <Container className="py-5 d-flex justify-content-center">
      <div className="w-100" style={{ maxWidth: '520px' }}>
        <div className="mb-4">
          <h3 className="fw-bold mb-1">Pago de Reservación</h3>
          <p className="text-muted mb-0">Reservación: <strong>{reservationCode}</strong></p>
        </div>

        {error && <Alert variant="danger" className="py-2">{error}</Alert>}

        <Card className="border-0 shadow-sm">
          <Card.Body className="p-4">
            <Form onSubmit={handleSubmit}>
              <Form.Group className="mb-3">
                <Form.Label className="text-muted small text-uppercase fw-bold">Monto a Pagar ($)</Form.Label>
                <Form.Control
                  type="number"
                  step="0.01"
                  min="0.01"
                  className="minimal-input"
                  placeholder="0.00"
                  value={form.amount}
                  onChange={set('amount')}
                  readOnly={!!montoParam}
                  required
                />
              </Form.Group>

              <hr className="my-3" />
              <p className="text-muted small fw-bold text-uppercase mb-3">Datos de Tarjeta</p>

              <Form.Group className="mb-3">
                <Form.Label className="text-muted small text-uppercase fw-bold">Número de Tarjeta</Form.Label>
                <Form.Control
                  type="text"
                  className="minimal-input"
                  placeholder="1234 5678 9012 3456"
                  maxLength={19}
                  value={form.cardNumber}
                  onChange={e => setForm(prev => ({
                    ...prev,
                    cardNumber: e.target.value.replace(/\D/g, '').replace(/(.{4})/g, '$1 ').trim(),
                  }))}
                  required
                />
              </Form.Group>

              <Form.Group className="mb-3">
                <Form.Label className="text-muted small text-uppercase fw-bold">Nombre del Titular</Form.Label>
                <Form.Control
                  type="text"
                  className="minimal-input"
                  placeholder="Como aparece en la tarjeta"
                  value={form.cardholderName}
                  onChange={set('cardholderName')}
                  required
                />
              </Form.Group>

              <Row>
                <Form.Group as={Col} md={6} className="mb-3">
                  <Form.Label className="text-muted small text-uppercase fw-bold">Vencimiento (MM/AA)</Form.Label>
                  <Form.Control
                    type="text"
                    className="minimal-input"
                    placeholder="MM/AA"
                    maxLength={5}
                    value={form.expirationDate}
                    onChange={e => {
                      let val = e.target.value.replace(/\D/g, '');
                      if (val.length >= 3) val = val.slice(0, 2) + '/' + val.slice(2, 4);
                      setForm(prev => ({ ...prev, expirationDate: val }));
                    }}
                    required
                  />
                </Form.Group>

                <Form.Group as={Col} md={6} className="mb-3">
                  <Form.Label className="text-muted small text-uppercase fw-bold">CVV</Form.Label>
                  <Form.Control
                    type="text"
                    className="minimal-input"
                    placeholder="123"
                    maxLength={4}
                    value={form.cvv}
                    onChange={e => setForm(prev => ({ ...prev, cvv: e.target.value.replace(/\D/g, '') }))}
                    required
                  />
                </Form.Group>
              </Row>

              <Button variant="dark" type="submit" className="w-100 py-2 mt-2" disabled={loading}>
                {loading ? 'Procesando pago...' : `Pagar $${Number(form.amount || 0).toFixed(2)}`}
              </Button>
            </Form>
          </Card.Body>
        </Card>

        <div className="text-center mt-3">
          <Button variant="link" className="text-muted small" onClick={() => navigate('/mis-reservaciones')}>
            Pagar después
          </Button>
        </div>
      </div>
    </Container>
  );
};

export default PaymentView;
