import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: () => (localStorage.getItem('token') ? '/home' : '/login'),
    },

    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { withoutAuth: true },
      beforeEnter: (to, from, next) => {
        const token = localStorage.getItem('token');
        if (token) {
          next('/home');
        } else {
          next();
        }
      },
    },
    {
      path: '/home',
      name: 'home',
      component: () => import('../views/HomeView.vue'),
    },
    {
      path: '/settings',
      name: 'settings',
      component: () => import('../views/AccountSettingsView.vue'),
    },
    {
      path: '/emailConfirmation',
      name: 'emailConfirmation',
      component: () => import('../views/EmailConfirmationView.vue'),
      meta: { withoutAuth: true },
    },
    {
      path: '/user/:fullName/:id',
      name: 'userProfilePublic',
      component: () => import('../views/UserProfileView.vue'),
      meta: { withoutAuth: true },
      props: route => ({
        fullName: route.params.fullName,
        id: Number(route.params.id)
      })
    },
    {
      path: '/:fullName/:id',
      name: 'userProfilePublicRoot',
      component: () => import('../views/UserProfileView.vue'),
      meta: { withoutAuth: true },
      props: route => ({
        fullName: route.params.fullName,
        id: Number(route.params.id)
      })
    },
  ],
})

router.beforeEach((to, from, next) => {

  if (to.meta.withoutAuth) return next();

  const token = localStorage.getItem('token');
  if (!token) return next('/login');

  return next();
});

export default router;
