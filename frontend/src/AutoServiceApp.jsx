import { useEffect, useState } from 'react'
import { api, session } from './api'

const emptyVehicle = { make: '', model: '', year: new Date().getFullYear(), licensePlate: '' }
const emptyService = { name: '', description: '', price: '', durationMinutes: 60, isActive: true }

function formatDate(value) {
  return new Intl.DateTimeFormat('en-GB', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function StatusBadge({ value }) {
  return <span className={`badge badge-${value.toLowerCase()}`}>{value}</span>
}

function Notice({ error, success }) {
  if (!error && !success) return null
  return <div className={`notice ${error ? 'notice-error' : 'notice-success'}`}>{error || success}</div>
}

function AuthPage({ onAuthenticated }) {
  const [registering, setRegistering] = useState(false)
  const [form, setForm] = useState({
    username: '', email: '', firstName: '', lastName: '', password: '',
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function submit(event) {
    event.preventDefault()
    setLoading(true)
    setError('')
    try {
      const result = await api(registering ? '/auth/register' : '/auth/login', {
        method: 'POST',
        body: JSON.stringify(registering ? form : {
          username: form.username,
          password: form.password,
        }),
      })
      session.token = result.token
      onAuthenticated(result.user)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="auth-shell">
      <section className="auth-hero">
        <p className="eyebrow">AUTO SERVICE BOOKING</p>
        <h1>Book maintenance without the phone calls.</h1>
        <p>Choose a service, register your vehicle, and manage every appointment in one place.</p>
        <div className="hero-points">
          <span>Verified service times</span>
          <span>Simple booking history</span>
          <span>Fast admin confirmation</span>
        </div>
      </section>
      <section className="auth-card">
        <div className="auth-tabs">
          <button className={!registering ? 'active' : ''} onClick={() => setRegistering(false)}>Sign in</button>
          <button className={registering ? 'active' : ''} onClick={() => setRegistering(true)}>Create account</button>
        </div>
        <form onSubmit={submit}>
          {registering && (
            <div className="form-grid">
              <label>First name<input required value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} /></label>
              <label>Last name<input required value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} /></label>
            </div>
          )}
          <label>Username<input required autoComplete="username" value={form.username} onChange={e => setForm({ ...form, username: e.target.value })} /></label>
          {registering && <label>Email<input required type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} /></label>}
          <label>Password<input required type="password" autoComplete={registering ? 'new-password' : 'current-password'} value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} /></label>
          <Notice error={error} />
          <button className="primary wide" disabled={loading}>{loading ? 'Please wait…' : registering ? 'Create account' : 'Sign in'}</button>
        </form>
        <p className="demo-note">Admin demo: <strong>admin</strong> / <strong>Admin123!</strong></p>
      </section>
    </main>
  )
}

function CustomerDashboard({ user }) {
  const [vehicles, setVehicles] = useState([])
  const [services, setServices] = useState([])
  const [bookings, setBookings] = useState([])
  const [vehicle, setVehicle] = useState(emptyVehicle)
  const [booking, setBooking] = useState({ vehicleId: '', serviceId: '', startTime: '', customerNote: '' })
  const [message, setMessage] = useState({ error: '', success: '' })

  async function load() {
    try {
      const [vehicleData, serviceData, bookingData] = await Promise.all([
        api('/vehicles'), api('/services'), api('/bookings'),
      ])
      setVehicles(vehicleData)
      setServices(serviceData)
      setBookings(bookingData)
    } catch (err) {
      setMessage({ error: err.message, success: '' })
    }
  }

  // Data loading is the external synchronization this effect is responsible for.
  // oxlint-disable-next-line react/set-state-in-effect
  useEffect(() => { void load() }, [])

  async function addVehicle(event) {
    event.preventDefault()
    try {
      await api('/vehicles', { method: 'POST', body: JSON.stringify({ ...vehicle, year: Number(vehicle.year) }) })
      setVehicle(emptyVehicle)
      setMessage({ success: 'Vehicle added.', error: '' })
      await load()
    } catch (err) {
      setMessage({ error: err.message, success: '' })
    }
  }

  async function removeVehicle(id) {
    if (!confirm('Delete this vehicle?')) return
    try {
      await api(`/vehicles/${id}`, { method: 'DELETE' })
      await load()
    } catch (err) {
      setMessage({ error: err.message, success: '' })
    }
  }

  async function createBooking(event) {
    event.preventDefault()
    try {
      await api('/bookings', {
        method: 'POST',
        body: JSON.stringify({
          ...booking,
          vehicleId: Number(booking.vehicleId),
          serviceId: Number(booking.serviceId),
          startTime: new Date(booking.startTime).toISOString(),
        }),
      })
      setBooking({ vehicleId: '', serviceId: '', startTime: '', customerNote: '' })
      setMessage({ success: 'Booking request sent.', error: '' })
      await load()
    } catch (err) {
      setMessage({ error: err.message, success: '' })
    }
  }

  async function cancelBooking(id) {
    try {
      await api(`/bookings/${id}/cancel`, { method: 'PUT' })
      await load()
    } catch (err) {
      setMessage({ error: err.message, success: '' })
    }
  }

  return (
    <>
      <section className="welcome">
        <div><p className="eyebrow">CUSTOMER DASHBOARD</p><h2>Welcome back, {user.firstName}.</h2></div>
        {user.isBlocked && <div className="notice notice-error">Your account is blocked from creating new bookings.</div>}
      </section>
      <Notice {...message} />
      <section className="stats">
        <article><strong>{vehicles.length}</strong><span>Vehicles</span></article>
        <article><strong>{bookings.filter(item => item.status === 'Pending').length}</strong><span>Pending</span></article>
        <article><strong>{bookings.filter(item => item.status === 'Confirmed').length}</strong><span>Confirmed</span></article>
      </section>
      <div className="dashboard-grid">
        <section className="panel">
          <div className="panel-heading"><div><p className="eyebrow">GARAGE</p><h3>Your vehicles</h3></div></div>
          <div className="item-list">
            {vehicles.map(item => (
              <article className="list-item" key={item.id}>
                <div><strong>{item.make} {item.model}</strong><small>{item.year} · {item.licensePlate}</small></div>
                <button className="danger-link" onClick={() => removeVehicle(item.id)}>Delete</button>
              </article>
            ))}
            {!vehicles.length && <p className="empty">Add your first vehicle.</p>}
          </div>
          <form className="compact-form" onSubmit={addVehicle}>
            <div className="form-grid">
              <label>Make<input required value={vehicle.make} onChange={e => setVehicle({ ...vehicle, make: e.target.value })} /></label>
              <label>Model<input required value={vehicle.model} onChange={e => setVehicle({ ...vehicle, model: e.target.value })} /></label>
              <label>Year<input required type="number" min="1950" value={vehicle.year} onChange={e => setVehicle({ ...vehicle, year: e.target.value })} /></label>
              <label>Plate<input required value={vehicle.licensePlate} onChange={e => setVehicle({ ...vehicle, licensePlate: e.target.value })} /></label>
            </div>
            <button className="secondary">Add vehicle</button>
          </form>
        </section>
        <section className="panel accent-panel">
          <p className="eyebrow">NEW APPOINTMENT</p><h3>Book a service</h3>
          <form onSubmit={createBooking}>
            <label>Vehicle<select required value={booking.vehicleId} onChange={e => setBooking({ ...booking, vehicleId: e.target.value })}><option value="">Choose a vehicle</option>{vehicles.map(item => <option key={item.id} value={item.id}>{item.make} {item.model} · {item.licensePlate}</option>)}</select></label>
            <label>Service<select required value={booking.serviceId} onChange={e => setBooking({ ...booking, serviceId: e.target.value })}><option value="">Choose a service</option>{services.map(item => <option key={item.id} value={item.id}>{item.name} — ₾{item.price} / {item.durationMinutes} min</option>)}</select></label>
            <label>Date and time<input required type="datetime-local" value={booking.startTime} onChange={e => setBooking({ ...booking, startTime: e.target.value })} /></label>
            <label>Note<textarea rows="3" value={booking.customerNote} onChange={e => setBooking({ ...booking, customerNote: e.target.value })} /></label>
            <p className="hint">Open Monday–Saturday, 09:00–18:00.</p>
            <button className="primary wide" disabled={user.isBlocked || !vehicles.length}>Request booking</button>
          </form>
        </section>
      </div>
      <section className="panel">
        <div className="panel-heading"><div><p className="eyebrow">HISTORY</p><h3>Your bookings</h3></div></div>
        <div className="table-wrap"><table><thead><tr><th>Appointment</th><th>Vehicle</th><th>Service</th><th>Status</th><th></th></tr></thead>
          <tbody>{bookings.map(item => <tr key={item.id}><td>{formatDate(item.startTime)}</td><td>{item.vehicle}<small>{item.licensePlate}</small></td><td>{item.serviceName}<small>₾{item.price}</small></td><td><StatusBadge value={item.status} /></td><td>{item.status === 'Pending' && <button className="danger-link" onClick={() => cancelBooking(item.id)}>Cancel</button>}</td></tr>)}</tbody>
        </table>{!bookings.length && <p className="empty">No bookings yet.</p>}</div>
      </section>
    </>
  )
}

function AdminDashboard() {
  const [bookings, setBookings] = useState([])
  const [services, setServices] = useState([])
  const [users, setUsers] = useState([])
  const [service, setService] = useState(emptyService)
  const [error, setError] = useState('')

  async function load() {
    try {
      const [bookingData, serviceData, userData] = await Promise.all([
        api('/bookings/admin'), api('/services/admin'), api('/admin/users'),
      ])
      setBookings(bookingData)
      setServices(serviceData)
      setUsers(userData)
      setError('')
    } catch (err) {
      setError(err.message)
    }
  }

  // Data loading is the external synchronization this effect is responsible for.
  // oxlint-disable-next-line react/set-state-in-effect
  useEffect(() => { void load() }, [])

  async function setStatus(id, status) {
    const adminComment = status === 'Rejected' ? prompt('Reason for rejection:') : null
    if (status === 'Rejected' && !adminComment) return
    try {
      await api(`/bookings/admin/${id}/status`, { method: 'PUT', body: JSON.stringify({ status, adminComment }) })
      await load()
    } catch (err) { setError(err.message) }
  }

  async function addService(event) {
    event.preventDefault()
    try {
      await api('/services', { method: 'POST', body: JSON.stringify({ ...service, price: Number(service.price), durationMinutes: Number(service.durationMinutes) }) })
      setService(emptyService)
      await load()
    } catch (err) { setError(err.message) }
  }

  async function toggleService(item) {
    try {
      await api(`/services/${item.id}`, { method: 'PUT', body: JSON.stringify({ ...item, isActive: !item.isActive }) })
      await load()
    } catch (err) { setError(err.message) }
  }

  async function toggleBlock(user) {
    try {
      await api(`/admin/users/${user.id}/block`, { method: 'PUT', body: JSON.stringify({ isBlocked: !user.isBlocked }) })
      await load()
    } catch (err) { setError(err.message) }
  }

  return (
    <>
      <section className="welcome"><div><p className="eyebrow">ADMIN DESK</p><h2>Service operations at a glance.</h2></div></section>
      <Notice error={error} />
      <section className="stats">
        <article><strong>{bookings.filter(x => x.status === 'Pending').length}</strong><span>Awaiting review</span></article>
        <article><strong>{bookings.filter(x => x.status === 'Confirmed').length}</strong><span>Confirmed</span></article>
        <article><strong>{users.filter(x => x.isBlocked).length}</strong><span>Blocked users</span></article>
      </section>
      <section className="panel">
        <p className="eyebrow">BOOKING QUEUE</p><h3>All appointments</h3>
        <div className="table-wrap"><table><thead><tr><th>Appointment</th><th>Customer</th><th>Vehicle</th><th>Service</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>{bookings.map(item => <tr key={item.id}><td>{formatDate(item.startTime)}</td><td>{item.customerName}</td><td>{item.vehicle}<small>{item.licensePlate}</small></td><td>{item.serviceName}</td><td><StatusBadge value={item.status} /></td><td className="actions">{item.status === 'Pending' && <><button onClick={() => setStatus(item.id, 'Confirmed')}>Confirm</button><button className="danger-link" onClick={() => setStatus(item.id, 'Rejected')}>Reject</button></>}{item.status === 'Confirmed' && <><button onClick={() => setStatus(item.id, 'Completed')}>Complete</button><button className="danger-link" onClick={() => setStatus(item.id, 'Cancelled')}>Cancel</button></>}</td></tr>)}</tbody>
        </table></div>
      </section>
      <div className="dashboard-grid">
        <section className="panel">
          <p className="eyebrow">CATALOG</p><h3>Services</h3>
          <div className="item-list">{services.map(item => <article className="list-item" key={item.id}><div><strong>{item.name}</strong><small>₾{item.price} · {item.durationMinutes} min · {item.isActive ? 'Active' : 'Inactive'}</small></div><button onClick={() => toggleService(item)}>{item.isActive ? 'Deactivate' : 'Activate'}</button></article>)}</div>
          <form className="compact-form" onSubmit={addService}>
            <label>Name<input required value={service.name} onChange={e => setService({ ...service, name: e.target.value })} /></label>
            <label>Description<textarea required value={service.description} onChange={e => setService({ ...service, description: e.target.value })} /></label>
            <div className="form-grid"><label>Price<input required type="number" min="1" step="0.01" value={service.price} onChange={e => setService({ ...service, price: e.target.value })} /></label><label>Minutes<input required type="number" min="15" value={service.durationMinutes} onChange={e => setService({ ...service, durationMinutes: e.target.value })} /></label></div>
            <button className="secondary">Add service</button>
          </form>
        </section>
        <section className="panel">
          <p className="eyebrow">CUSTOMERS</p><h3>User access</h3>
          <div className="item-list">{users.map(user => <article className="list-item" key={user.id}><div><strong>{user.firstName} {user.lastName}</strong><small>{user.username} · {user.email}</small></div><button className={user.isBlocked ? '' : 'danger-link'} onClick={() => toggleBlock(user)}>{user.isBlocked ? 'Unblock' : 'Block'}</button></article>)}</div>
        </section>
      </div>
    </>
  )
}

export default function AutoServiceApp() {
  const [user, setUser] = useState(null)
  const [ready, setReady] = useState(!session.token)

  useEffect(() => {
    if (!session.token) return
    api('/auth/me').then(setUser).catch(() => { session.token = null }).finally(() => setReady(true))
  }, [])

  function logout() {
    session.token = null
    setUser(null)
  }

  if (!ready) return <div className="loading">Loading AutoHub…</div>
  if (!user) return <AuthPage onAuthenticated={setUser} />

  return (
    <div className="app-shell">
      <header>
        <a className="brand" href="#"><span>AH</span> AutoHub</a>
        <div className="user-menu"><div><strong>{user.firstName} {user.lastName}</strong><small>{user.role}</small></div><button onClick={logout}>Sign out</button></div>
      </header>
      <main className="content">{user.role === 'Admin' ? <AdminDashboard /> : <CustomerDashboard user={user} />}</main>
      <footer>AutoHub Service Booking · Portfolio project</footer>
    </div>
  )
}
