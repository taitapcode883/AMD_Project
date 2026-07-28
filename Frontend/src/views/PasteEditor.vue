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
      <section class="paste-heading">
        <div>
          <p class="paste-eyebrow">New paste</p>
          <h2 class="paste-page-title">Create a new snippet</h2>
          <p class="paste-page-description">
            Paste your code, choose its settings and share the generated link.
          </p>
        </div>
      </section>

      <form class="editor-card" novalidate @submit.prevent="createPaste">
        <div class="editor-field">
          <div class="editor-label-row">
            <label class="form-label" for="paste-content">Content</label>
            <span class="character-count" :class="{ 'character-count-error': contentTooLarge }">
              {{ formattedSize }} / 500 KB
            </span>
          </div>

          <textarea
            id="paste-content"
            ref="contentInput"
            v-model="form.content"
            class="code-editor"
            :class="{ 'input-invalid': contentTooLarge }"
            spellcheck="false"
            placeholder="Paste your code or text here..."
            @keydown.tab.prevent="insertTab"
          />
        </div>

        <div class="editor-options">
          <div class="form-group">
            <label class="form-label" for="language">Language</label>
            <select id="language" v-model="form.language" class="form-select">
              <option v-for="language in languages" :key="language" :value="language">
                {{ language }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label" for="visibility">Visibility</label>
            <select id="visibility" v-model="form.visibility" class="form-select">
              <option value="public">Public</option>
              <option value="private">Private</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label" for="expiry">Expiry</label>
            <select id="expiry" v-model="form.expiry" class="form-select">
              <option value="never">Never</option>
              <option value="10m">10 minutes</option>
              <option value="1h">1 hour</option>
              <option value="1d">1 day</option>
              <option value="1w">1 week</option>
            </select>
          </div>
        </div>

        <div class="editor-actions">
          <button class="secondary-button editor-cancel" type="button" @click="cancel">
            Cancel
          </button>
          <button class="primary-button editor-submit" type="submit" :disabled="isSubmitting">
            {{ isSubmitting ? "Creating..." : "Create paste" }}
          </button>
        </div>
      </form>
    </div>
  </main>
</template>

<script>
import Notification from "../components/Notification.vue";

const API_BASE_URL = import.meta.env.VITE_API_URL || "";
const MAX_CONTENT_SIZE = 500 * 1024;

export default {
  name: "PasteEditor",

  components: { Notification },

  data() {
    return {
      form: {
        content: "",
        language: "Plain Text",
        visibility: "public",
        expiry: "never"
      },
      languages: [
        "Plain Text", "JavaScript", "TypeScript", "HTML", "CSS", "Vue",
        "C#", "Java", "Python", "PHP", "SQL", "JSON", "Bash"
      ],
      isSubmitting: false,
      notification: {
        show: false,
        type: "success",
        title: "",
        message: ""
      }
    };
  },

  computed: {
    contentSize() {
      return new Blob([this.form.content]).size;
    },

    contentTooLarge() {
      return this.contentSize > MAX_CONTENT_SIZE;
    },

    formattedSize() {
      if (this.contentSize < 1024) return `${this.contentSize} B`;
      return `${(this.contentSize / 1024).toFixed(1)} KB`;
    }
  },

  methods: {
    notify(type, title, message) {
      this.notification = { show: true, type, title, message };
      window.setTimeout(() => { this.notification.show = false; }, 3000);
    },

    insertTab(event) {
      const textarea = event.target;
      const start = textarea.selectionStart;
      const end = textarea.selectionEnd;
      this.form.content = `${this.form.content.slice(0, start)}  ${this.form.content.slice(end)}`;

      this.$nextTick(() => {
        textarea.selectionStart = textarea.selectionEnd = start + 2;
      });
    },

    async createPaste() {
      if (!this.form.content.trim()) {
        this.notify("error", "Content required", "Please enter some code or text.");
        this.$refs.contentInput.focus();
        return;
      }

      if (this.contentTooLarge) {
        this.notify("error", "Paste too large", "Content must not exceed 500 KB.");
        return;
      }

      this.isSubmitting = true;

      try {
        const response = await fetch(`${API_BASE_URL}/pastes`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            code: "",
            content: this.form.content,
            language: this.form.language,
            visibility: this.form.visibility,
            createdAt: new Date().toISOString(),
            expiry: this.form.expiry,
            ownerId: null,
            viewCount: 0
          })
        });

        if (!response.ok) throw new Error(`Request failed (${response.status})`);

        const paste = await response.json();
        const code = paste.code || paste.Code;
        const id = paste.id || paste.Id;

        this.notify("success", "Paste created", "Your snippet is ready to share.");
        window.setTimeout(() => this.$router.push(`/paste/${code || id}`), 700);
      } catch (error) {
        this.notify(
          "error",
          "Could not create paste",
          "Check that the API Gateway is running on port 5179."
        );
      } finally {
        this.isSubmitting = false;
      }
    },

    cancel() {
      this.$router.push("/dashboard");
    }
  }
};
</script>
