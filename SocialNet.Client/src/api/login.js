import api from './api';

export async function doLogin(body) {
	return api.post(`/Identity/Login`,body);
}

export async function signUp(signUpData) {
	return api.post(`/Identity/Register`, signUpData,
		{
			skipAuth: true
		}
	);
}

export async function confirmEmail(email, token) {
	return api.post(`/Identity/ConfirmEmail`, 
	{
		Email: email,
		Token: token
	});
}