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
      <h1 class="auth-title">Welcome back</h1>

      <p class="auth-description">
        Login to manage your pastes.
      </p>

      <form
        class="auth-form"
        novalidate
        @submit.prevent="handleLogin"
      >
        <div class="form-group">
          <label class="form-label">Email</label>

          <input
            v-model.trim="email"
            class="form-input"
            type="email"
            placeholder="Enter your email"
          />
        </div>

        <div class="form-group">
          <label class="form-label">Password</label>

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
        >
          Login
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

export default {
  name: "Login",

  components: {
    Notification
  },

  data() {
    return {
      email: "",
      password: "",

      notification: {
        show: false,
        type: "success",
        title: "",
        message: ""
      }
    };
  },

  methods: {
    notify(type, title, message) {
      this.notification = {
        show: true,
        type,
        title,
        message
      };

      setTimeout(() => {
        this.notification.show = false;
      }, 2500);
    },

    handleLogin() {
      if (!this.email || !this.password) {
        return this.notify(
          "error",
          "Login failed",
          "Please complete all fields."
        );
      }

      const validEmail =
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email);

      if (!validEmail) {
        return this.notify(
          "error",
          "Invalid email",
          "Please enter a valid email address."
        );
      }

      const user = JSON.parse(
        localStorage.getItem("registeredUser")
      );

      if (
        !user ||
        user.email !== this.email ||
        user.password !== this.password
      ) {
        return this.notify(
          "error",
          "Login failed",
          "Email or password is incorrect."
        );
      }

      localStorage.setItem("isLoggedIn", "true");

      this.notify(
        "success",
        "Login successful",
        "Welcome back."
      );

      setTimeout(() => {
        this.$router.push("/dashboard");
      }, 1000);
    }
  }
};
</script>