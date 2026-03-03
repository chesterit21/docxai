import Breadcrumb  from '../components/atom/Breadcrumb';
import NotificationComponent from '../components/molecule/notification'

const Notification = () => {
    return (
        <>
            <Breadcrumb item={[{title: <span style={{fontWeight: 'bold'}}>Notification</span>}]}/>
            <NotificationComponent />
        </>        
    )
}
export default Notification;