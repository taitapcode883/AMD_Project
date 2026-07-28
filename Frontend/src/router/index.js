import {
  createRouter,
  createWebHistory
} from "vue-router";

import Login from "../views/Login.vue";
import Register from "../views/Register.vue";
import Dashboard from "../views/Dashboard.vue";
import PasteEditor from "../views/PasteEditor.vue";
import PasteView from "../views/PasteView.vue";

const routes = [
  {
    path: "/",
    redirect: "/login"
  },
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
    component: Dashboard
  },
  {
    path: "/paste/new",
    name: "PasteEditor",
    component: PasteEditor
  },
  {
    path: "/paste/:code",
    name: "PasteView",
    component: PasteView
  },
  {
    path: "/:pathMatch(.*)*",
    redirect: "/login"
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
