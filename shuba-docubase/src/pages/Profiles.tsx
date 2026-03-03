import Breadcrumb  from '../components/atom/Breadcrumb';
import ProfileComponent from '../components/molecule/profiles'
const Profiles = () => {
    return (
        <>
            <Breadcrumb item={[{title: 'Profiles'}, {title: <span className='font-bold'>View</span>}]}/>
            <ProfileComponent />
        </>        
    )
}
export default Profiles;