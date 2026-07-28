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
      <h1 class="auth-title">Create account</h1>

      <p class="auth-description">
        Register to create and manage your pastes.
      </p>

      <form
        class="auth-form"
        novalidate
        @submit.prevent="handleRegister"
      >
        <div class="form-group">
          <label class="form-label">Username</label>

          <input
            v-model.trim="username"
            class="form-input"
            type="text"
            placeholder="Enter your username"
          />
        </div>

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

        <div class="form-group">
          <label class="form-label">Confirm password</label>

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
        >
          Register
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

    handleRegister() {
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

      const validEmail =
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email);

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

      const user = {
        username: this.username,
        email: this.email,
        password: this.password
      };

      localStorage.setItem(
        "registeredUser",
        JSON.stringify(user)
      );

      this.notify(
        "success",
        "Registration successful",
        "Your account has been created."
      );

      setTimeout(() => {
        this.$router.push("/login");
      }, 1200);
    }
  }
};
</script>