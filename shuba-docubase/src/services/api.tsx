import axios from 'axios'

const api = axios.create({
  baseURL: process.env.REACT_APP_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
    'x-lang':  `${localStorage.getItem('i18nextLng') || 'en'}`
  },
})

export default api
