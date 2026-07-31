<template>
  <main class="auth-page">
    <Notification
      :show="notification.show"
      :type="notification.type"
      :title="notification.title"
      :message="notification.message"
      @close="notification.show = false"
    />

    <section class="auth-card">
      <h1 class="auth-title">
        Welcome back
      </h1>

      <p class="auth-description">
        Login to manage your pastes.
      </p>

      <form
        class="auth-form"
        novalidate
        @submit.prevent="handleLogin"
      >
        <div class="form-group">
          <label class="form-label">
            Email
          </label>

          <input
            v-model.trim="email"
            class="form-input"
            type="email"
            placeholder="Enter your email"
          />
        </div>

        <div class="form-group">
          <label class="form-label">
            Password
          </label>

          <input
            v-model="password"
            class="form-input"
            type="password"
            placeholder="Enter your password"
          />
        </div>

        <button
          class="primary-button"
          type="submit"
          :disabled="loading"
        >
          {{ loading ? "Logging in..." : "Login" }}
        </button>
      </form>

      <p class="auth-footer">
        Don't have an account?

        <router-link to="/register">
          Register
        </router-link>
      </p>
    </section>
  </main>
</template>

<script>
import Notification from "../components/Notification.vue";

import {
  apiRequest,
  saveSession
} from "../services/api.js";

export default {
  name: "Login",

  components: {
    Notification
  },

  data() {
    return {
      email: "",
      password: "",
      loading: false,

      notification: {
        show: false,
        type: "success",
        title: "",
        message: ""
      },

      notificationTimer: null,
      redirectTimer: null
    };
  },

  beforeUnmount() {
    clearTimeout(this.notificationTimer);
    clearTimeout(this.redirectTimer);
  },

  methods: {
    notify(type, title, message) {
      clearTimeout(this.notificationTimer);

      this.notification = {
        show: true,
        type,
        title,
        message
      };

      this.notificationTimer = setTimeout(() => {
        this.notification.show = false;
      }, 2500);
    },

    async handleLogin() {
      if (!this.email || !this.password) {
        return this.notify(
          "error",
          "Login failed",
          "Please complete all fields."
        );
      }

      const email = this.email.trim().toLowerCase();

      const validEmail =
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

      if (!validEmail) {
        return this.notify(
          "error",
          "Invalid email",
          "Please enter a valid email address."
        );
      }

      try {
        this.loading = true;

        const response = await apiRequest("/auth/login", {
          method: "POST",

          body: JSON.stringify({
            email: email,
            password: this.password
          })
        });

        console.log("Login response:", response);

        if (!response || !response.token) {
          throw new Error(
            "Backend did not return a login token."
          );
        }

        saveSession(response);

        this.notify(
          "success",
          "Login successful",
          `Welcome back, ${response.user?.username || "User"}.`
        );

        clearTimeout(this.redirectTimer);

        this.redirectTimer = setTimeout(() => {
          this.$router.push("/dashboard");
        }, 1000);
      } catch (error) {
        console.error("Login error:", error);

        this.notify(
          "error",
          "Login failed",
          error.message ||
            "Email or password is incorrect."
        );
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>