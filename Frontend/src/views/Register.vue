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
        Create account
      </h1>

      <p class="auth-description">
        Register to create and manage your pastes.
      </p>

      <form
        class="auth-form"
        novalidate
        @submit.prevent="handleRegister"
      >
        <div class="form-group">
          <label class="form-label">
            Username
          </label>

          <input
            v-model.trim="username"
            class="form-input"
            type="text"
            placeholder="Enter your username"
          />
        </div>

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

        <div class="form-group">
          <label class="form-label">
            Confirm password
          </label>

          <input
            v-model="confirmPassword"
            class="form-input"
            type="password"
            placeholder="Confirm your password"
          />
        </div>

        <button
          class="primary-button"
          type="submit"
          :disabled="loading"
        >
          {{ loading ? "Registering..." : "Register" }}
        </button>
      </form>

      <p class="auth-footer">
        Already have an account?

        <router-link to="/login">
          Login
        </router-link>
      </p>
    </section>
  </main>
</template>

<script>
import Notification from "../components/Notification.vue";
import { apiRequest } from "../services/api.js";

export default {
  name: "Register",

  components: {
    Notification
  },

  data() {
    return {
      username: "",
      email: "",
      password: "",
      confirmPassword: "",
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

    async handleRegister() {
      if (
        !this.username ||
        !this.email ||
        !this.password ||
        !this.confirmPassword
      ) {
        return this.notify(
          "error",
          "Registration failed",
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

      if (this.password.length < 6) {
        return this.notify(
          "error",
          "Invalid password",
          "Password must contain at least 6 characters."
        );
      }

      if (this.password !== this.confirmPassword) {
        return this.notify(
          "error",
          "Passwords do not match",
          "Please enter the same password again."
        );
      }

      try {
        this.loading = true;

        const response = await apiRequest(
          "/auth/register",
          {
            method: "POST",

            body: JSON.stringify({
              username: this.username.trim(),
              email: email,
              password: this.password
            })
          }
        );

        console.log("Register response:", response);

        this.notify(
          "success",
          "Registration successful",
          "Your account has been created."
        );

        clearTimeout(this.redirectTimer);

        this.redirectTimer = setTimeout(() => {
          this.$router.push("/login");
        }, 1200);
      } catch (error) {
        console.error("Register error:", error);

        this.notify(
          "error",
          "Registration failed",
          error.message || "Could not create account."
        );
      } finally {
        this.loading = false;
      }
    }
  }
};
</script>