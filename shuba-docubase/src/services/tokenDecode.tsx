import { jwtDecode } from 'jwt-decode';

interface TokenAuthPayload{
  aud: string;
  company_id: string;
  company_name: string;
  email: string;
  exp: number;
  family_name: string;
  given_name: string;
  iat: null;
  iss: string;
  name: string;
  sub: string;
  user_type: string;
  website: string;
}

export const getDecodedToken = (): TokenAuthPayload | null => {
    const token = localStorage.getItem('user');

    if (!token) {
      console.log('Token not found in local storage.');
      return null;
    }

    try {
      const decodedToken = jwtDecode<TokenAuthPayload>(token); 
      return decodedToken;
    } catch (error) {
      console.log('Invalid token:', error);
      return null;
    }
  };