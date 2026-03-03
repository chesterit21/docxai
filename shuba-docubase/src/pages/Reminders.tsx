import Breadcrumb from "../components/atom/Breadcrumb";
import ReminderComponent from "../components/molecule/reminder";

const Reminders = () => {
  return (
    <>
      <Breadcrumb
        item={[{ title: <span style={{ fontWeight: "bold" }}>Reminder</span> }]}
      />
      <ReminderComponent />
    </>
  );
};
export default Reminders;
