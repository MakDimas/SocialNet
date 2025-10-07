import api from './api';

export async function generateCaptcha() {
  const { data } = await api.get('/Captcha/Generate', { skipAuth: true });
  return data.data;
}

export async function validateCaptcha({ captchaId, input }) {
  const { data } = await api.post('/Captcha/Validate', { CaptchaId: captchaId, Input: input }, { skipAuth: true });
  return data.data;
}


