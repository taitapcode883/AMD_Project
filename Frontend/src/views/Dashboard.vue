<template>
  <main class="dashboard-page">
    <Notification
      :show="notification.show"
      :type="notification.type"
      :title="notification.title"
      :message="notification.message"
      @close="notification.show = false"
    />

    <div class="dashboard-container">
      <section class="statistics-box">
        <article class="stat-item">
          <div class="stat-content">
            <div>
              <p class="stat-label">
                Total pastes
              </p>

              <strong class="stat-number">
                {{ pastes.length }}
              </strong>
            </div>

            <ChartNoAxesCombined class="stat-icon" />
          </div>
        </article>

        <article class="stat-item">
          <div class="stat-content">
            <div>
              <p class="stat-label">
                Public pastes
              </p>

              <strong class="stat-number">
                {{ publicPasteCount }}
              </strong>
            </div>

            <Globe2 class="stat-icon" />
          </div>
        </article>

        <article class="stat-item">
          <div class="stat-content">
            <div>
              <p class="stat-label">
                Private pastes
              </p>

              <strong class="stat-number">
                {{ privatePasteCount }}
              </strong>
            </div>

            <LockKeyhole class="stat-icon" />
          </div>
        </article>
      </section>

      <section class="dashboard-card">
        <div class="dashboard-toolbar">
          <div>
            <h2 class="dashboard-toolbar-title">
              My pastes
            </h2>

            <p class="dashboard-toolbar-description">
              View and manage your real pastes.
            </p>
          </div>

          <input
            v-model="searchText"
            class="search-input"
            type="search"
            placeholder="Search pastes..."
          />
        </div>

        <div
          v-if="loading"
          class="empty-state"
        >
          Loading your pastes...
        </div>

        <div
          v-else-if="filteredPastes.length === 0"
          class="empty-state"
        >
          {{ searchText ? "No pastes match your search." : "You do not have any pastes yet." }}
        </div>

        <div
          v-else
          class="table-wrapper"
        >
          <table class="paste-table">
            <thead>
              <tr>
                <th>Paste</th>
                <th>Language</th>
                <th>Visibility</th>
                <th>Created</th>
                <th>Expiry</th>
                <th>Actions</th>
              </tr>
            </thead>

            <tbody>
              <tr
                v-for="paste in filteredPastes"
                :key="paste.id || paste.code"
                class="paste-table-row"
              >
                <td>
                  <p class="paste-title">
                    {{ getPasteTitle(paste) }}
                  </p>

                  <span class="paste-code">
                    /paste/{{ paste.code }}
                  </span>
                </td>

                <td>
                  {{ paste.language || "Plain text" }}
                </td>

                <td>
                  <span
                    :class="
                      isPublic(paste.visibility)
                        ? 'public-badge'
                        : 'private-badge'
                    "
                  >
                    {{ formatVisibility(paste.visibility) }}
                  </span>
                </td>

                <td>
                  {{ formatDate(paste.createdAt) }}
                </td>

                <td>
                  {{ formatExpiry(paste.expiresAt) }}
                </td>

                <td>
                  <div class="table-actions">
                    <button
                      class="secondary-button"
                      type="button"
                      @click="viewPaste(paste.code)"
                    >
                      View
                    </button>

                    <button
                      class="danger-button"
                      type="button"
                      :disabled="
                        deletingCode === paste.code
                      "
                      @click="deletePaste(paste.code)"
                    >
                      {{
                        deletingCode === paste.code
                          ? "Deleting..."
                          : "Delete"
                      }}
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </main>
</template>

<script>
import {
  ChartNoAxesCombined,
  Globe2,
  LockKeyhole
} from "lucide-vue-next";

import Notification from "../components/Notification.vue";
import { apiRequest } from "../services/api.js";

export default {
  name: "Dashboard",

  components: {
    ChartNoAxesCombined,
    Globe2,
    LockKeyhole,
    Notification
  },

  data() {
    return {
      searchText: "",
      pastes: [],
      loading: false,
      deletingCode: "",

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

  computed: {
    filteredPastes() {
      const keyword = this.searchText
        .trim()
        .toLowerCase();

      if (!keyword) {
        return this.pastes;
      }

      return this.pastes.filter((paste) => {
        const title =
          paste.title ||
          this.getPasteTitle(paste);

        const language =
          paste.language || "";

        const code =
          paste.code || "";

        const content =
          paste.content || "";

        return (
          title.toLowerCase().includes(keyword) ||
          language.toLowerCase().includes(keyword) ||
          code.toLowerCase().includes(keyword) ||
          content.toLowerCase().includes(keyword)
        );
      });
    },

    publicPasteCount() {
      return this.pastes.filter((paste) =>
        this.isPublic(paste.visibility)
      ).length;
    },

    privatePasteCount() {
  return this.pastes.filter(
    (paste) =>
      String(paste.visibility).toLowerCase() === "private"
  ).length;
}
  },

  mounted() {
    this.fetchPastes();
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
      }, 3500);
    },

    async fetchPastes() {
      try {
        this.loading = true;

        const response = await apiRequest(
          "/pastes/mine"
        );

        console.log(
          "Pastes response:",
          response
        );

        if (Array.isArray(response)) {
          this.pastes = response;
        } else if (
          Array.isArray(response?.pastes)
        ) {
          this.pastes = response.pastes;
        } else if (
          Array.isArray(response?.data)
        ) {
          this.pastes = response.data;
        } else {
          this.pastes = [];
        }
      } catch (error) {
        console.error(
          "Load pastes error:",
          error
        );

        if (error.status === 401) {
          this.notify(
            "error",
            "Session expired",
            "Please log in again."
          );

          clearTimeout(this.redirectTimer);

          this.redirectTimer = setTimeout(() => {
            this.$router.push("/login");
          }, 1500);

          return;
        }

        this.notify(
          "error",
          "Could not load pastes",
          error.message
        );
      } finally {
        this.loading = false;
      }
    },

    viewPaste(code) {
      if (!code) {
        return this.notify(
          "error",
          "View failed",
          "Paste code is missing."
        );
      }

      this.$router.push(`/paste/${code}`);
    },

    async deletePaste(code) {
      if (!code) {
        return this.notify(
          "error",
          "Delete failed",
          "Paste code is missing."
        );
      }

      const confirmed = window.confirm(
        "Are you sure you want to delete this paste?"
      );

      if (!confirmed) {
        return;
      }

      try {
        this.deletingCode = code;

        await apiRequest(`/pastes/${code}`, {
          method: "DELETE"
        });

        this.pastes = this.pastes.filter(
          (paste) => paste.code !== code
        );

        this.notify(
          "success",
          "Paste deleted",
          "The paste was deleted successfully."
        );
      } catch (error) {
        console.error(
          "Delete paste error:",
          error
        );

        this.notify(
          "error",
          "Delete failed",
          error.message
        );
      } finally {
        this.deletingCode = "";
      }
    },

    isPublic(visibility) {
      return String(visibility)
        .toLowerCase() === "public";
    },

    getPasteTitle(paste) {
      if (paste.title) {
        return paste.title;
      }

      if (!paste.content) {
        return "Untitled paste";
      }

      const firstLine = paste.content
        .split("\n")[0]
        .trim();

      if (!firstLine) {
        return "Untitled paste";
      }

      return firstLine.length > 35
        ? `${firstLine.slice(0, 35)}...`
        : firstLine;
    },

    formatVisibility(visibility) {
      if (!visibility) {
        return "Unknown";
      }

      const value =
        String(visibility).toLowerCase();

      return (
        value.charAt(0).toUpperCase() +
        value.slice(1)
      );
    },

    formatDate(date) {
      if (!date) {
        return "Unknown";
      }

      const value = new Date(date);

      if (Number.isNaN(value.getTime())) {
        return "Unknown";
      }

      return value.toLocaleDateString(
        "en-GB",
        {
          day: "2-digit",
          month: "short",
          year: "numeric"
        }
      );
    },

    formatExpiry(expiresAt) {
      if (!expiresAt) {
        return "Never";
      }

      const expiryDate =
        new Date(expiresAt);

      if (
        Number.isNaN(
          expiryDate.getTime()
        )
      ) {
        return "Unknown";
      }

      const difference =
        expiryDate.getTime() -
        Date.now();

      if (difference <= 0) {
        return "Expired";
      }

      const hours = Math.ceil(
        difference /
        (1000 * 60 * 60)
      );

      if (hours <= 24) {
        return `${hours} hour${
          hours === 1 ? "" : "s"
        }`;
      }

      const days = Math.ceil(
        hours / 24
      );

      return `${days} day${
        days === 1 ? "" : "s"
      }`;
    }
  }
};
</script>