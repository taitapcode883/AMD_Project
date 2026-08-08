import { createRouter, createWebHistory } from "vue-router";

import Login from "../views/Login.vue";
import Register from "../views/Register.vue";
import Dashboard from "../views/Dashboard.vue";
import PasteEditor from "../views/PasteEditor.vue";
import PasteView from "../views/PasteView.vue";

const routes = [
  {
    path: "/login",
    name: "Login",
    component: Login
  },
  {
    path: "/register",
    name: "Register",
    component: Register
  },
  {
    path: "/dashboard",
    name: "Dashboard",
    component: Dashboard,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/paste/new",
    name: "PasteEditor",
    component: PasteEditor,
    meta: {
      requiresAuth: true
    }
  },
  {
    path: "/paste/:code",
    name: "PasteView",
    component: PasteView
  },
  {
    path: "/",
    redirect: "/dashboard"
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});


router.beforeEach((to) => {
  const token = localStorage.getItem("token");

  if (to.meta.requiresAuth && !token) {
    return "/login";
  }

  return true;
});

export default router;