import api from './api';

export async function getUserByFullNameAndId({ fullName, id }) {
  return api.get(`/User/GetUserByFullNameAndId`, {
    params: { FullName: fullName, Id: id }
  });
}

export async function updateUser(payload) {
  return api.put(`/User/UpdateUser`, payload);
}
