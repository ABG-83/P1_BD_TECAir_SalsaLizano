import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { UserRole } from '../types';

interface Props {
  children: React.ReactNode;
  requiredRole?: UserRole | UserRole[];
  redirectTo?: string;
}

const ProtectedRoute = ({ children, requiredRole, redirectTo = '/login' }: Props) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) return <Navigate to={redirectTo} replace />;

  if (requiredRole) {
    const allowed = Array.isArray(requiredRole) ? requiredRole : [requiredRole];
    if (!allowed.includes(user!.rol)) return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
