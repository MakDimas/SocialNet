import api from './api';

export async function createPost(signUpData) {
	return api.post(`/Post/CreatePost`, signUpData);
}

export async function getPosts(queryParams) {
  return api.get(`/Post/GetPostsWithPagination`, { params: queryParams });
}
