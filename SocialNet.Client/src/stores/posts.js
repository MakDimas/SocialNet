import { defineStore } from 'pinia';
import { createPost as apiCreatePost, getPosts as apiGetPosts } from '@/api/post.js';

export const usePostStore = defineStore('post', {
    actions: {
        async createPost(createPostData) {
            let response = await apiCreatePost(createPostData);

            if (response.data.status == 200) return true;
            else return false;
        },
        async getPosts(parameters) {
            const response = await apiGetPosts(parameters);
            return response?.data;
        }
    }
});
