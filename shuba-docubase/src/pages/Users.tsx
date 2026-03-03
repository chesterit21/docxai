
import { Outlet } from 'react-router-dom';
import Page404 from '../pages/NotFoundGuest';
import { useAuth } from '../context/AuthContext';

const Users = () => {
    const { user } = useAuth();
    
    return (
        user?.user_type === 'superadmin' ? <Outlet /> : <Page404 />
    )
}
export default Users;