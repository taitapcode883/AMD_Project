<template>
  <main class="paste-page">
    <Notification
      :show="notification.show"
      :type="notification.type"
      :title="notification.title"
      :message="notification.message"
      @close="notification.show = false"
    />

    <div class="paste-container">
      <div v-if="isLoading" class="paste-state-card">Loading paste...</div>

      <div v-else-if="errorMessage" class="paste-state-card paste-error-state">
        <h2>Paste not available</h2>
        <p>{{ errorMessage }}</p>
        <router-link class="secondary-button state-link" to="/paste/new">
          Create a new paste
        </router-link>
      </div>

      <template v-else>
        <section class="paste-heading view-heading">
          <div>
            <p class="paste-eyebrow">Paste / {{ paste.code || paste.id }}</p>
            <h2 class="paste-page-title">{{ paste.language || "Plain Text" }} snippet</h2>
            <p class="paste-page-description">
              Created {{ formatDate(paste.createdAt) }}
            </p>
          </div>

          <div class="view-actions">
            <button class="secondary-button" type="button" @click="copyLink">
              Copy link
            </button>
            <button class="primary-button copy-button" type="button" @click="copyContent">
              Copy code
            </button>
          </div>
        </section>

        <section class="paste-meta-card">
          <div class="meta-item">
            <span>Language</span>
            <strong>{{ paste.language || "Plain Text" }}</strong>
          </div>
          <div class="meta-item">
            <span>Visibility</span>
            <strong :class="paste.visibility === 'private' ? 'meta-private' : 'meta-public'">
              {{ capitalize(paste.visibility || "public") }}
            </strong>
          </div>
          <div class="meta-item">
            <span>Expires</span>
            <strong>{{ formatExpiry(paste.expiresAt) }}</strong>
          </div>
          <div class="meta-item">
            <span>Views</span>
            <strong>{{ paste.viewCount || 0 }}</strong>
          </div>
        </section>

        <section class="code-card">
          <div class="code-card-header">
            <span>{{ paste.language || "Plain Text" }}</span>
            <span>{{ lineCount }} {{ lineCount === 1 ? "line" : "lines" }}</span>
          </div>
          <pre class="code-content"><code>{{ paste.content }}</code></pre>
        </section>
      </template>
    </div>
  </main>
</template>

<script>
import Notification from "../components/Notification.vue";

const API_BASE_URL = import.meta.env.VITE_API_URL || "";

export default {
  name: "PasteView",

  components: { Notification },

  data() {
    return {
      paste: null,
      isLoading: true,
      errorMessage: "",
      notification: {
        show: false,
        type: "success",
        title: "",
        message: ""
      }
    };
  },

  computed: {
    lineCount() {
      return this.paste?.content ? this.paste.content.split("\n").length : 0;
    }
  },

  created() {
    this.loadPaste();
  },

  methods: {
    normalizePaste(data) {
      return {
        id: data.id ?? data.Id,
        code: data.code ?? data.Code,
        content: data.content ?? data.Content ?? "",
        language: data.language ?? data.Language,
        visibility: (data.visibility ?? data.Visibility ?? "public").toLowerCase(),
        createdAt: data.createdAt ?? data.CreatedAt,
        expiresAt: data.expiresAt ?? data.ExpiresAt,
        viewCount: data.viewCount ?? data.ViewCount ?? 0
      };
    },

    async loadPaste() {
      this.isLoading = true;
      this.errorMessage = "";

      try {
        const response = await fetch(
          `${API_BASE_URL}/pastes/${encodeURIComponent(this.$route.params.code)}`
        );

        if (response.status === 404) throw new Error("This paste does not exist or has expired.");
        if (!response.ok) throw new Error("The paste service is currently unavailable.");

        this.paste = this.normalizePaste(await response.json());

        if (this.paste.expiresAt && new Date(this.paste.expiresAt) <= new Date()) {
          throw new Error("This paste has expired.");
        }
      } catch (error) {
        this.paste = null;
        this.errorMessage = error.message || "Unable to load this paste.";
      } finally {
        this.isLoading = false;
      }
    },

    showCopied(message) {
      this.notification = {
        show: true,
        type: "success",
        title: "Copied",
        message
      };
      window.setTimeout(() => { this.notification.show = false; }, 2200);
    },

    async copyContent() {
      try {
        await navigator.clipboard.writeText(this.paste.content);
        this.showCopied("Paste content copied to clipboard.");
      } catch {
        this.notification = {
          show: true,
          type: "error",
          title: "Copy failed",
          message: "Your browser did not allow clipboard access."
        };
      }
    },

    async copyLink() {
      try {
        await navigator.clipboard.writeText(window.location.href);
        this.showCopied("Share link copied to clipboard.");
      } catch {
        this.notification = {
          show: true,
          type: "error",
          title: "Copy failed",
          message: "Your browser did not allow clipboard access."
        };
      }
    },

    formatDate(value) {
      if (!value) return "Unknown";
      return new Intl.DateTimeFormat("en-GB", {
        day: "numeric", month: "long", year: "numeric", hour: "2-digit", minute: "2-digit"
      }).format(new Date(value));
    },

    formatExpiry(value) {
      return value ? this.formatDate(value) : "Never";
    },

    capitalize(value) {
      return value.charAt(0).toUpperCase() + value.slice(1);
    }
  }
};
</script>
