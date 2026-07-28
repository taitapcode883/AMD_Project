<template>
  <main class="dashboard-page">
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
              Recent snippets
            </h2>

            <p class="dashboard-toolbar-description">
              Search, view or delete your snippets.
            </p>
          </div>

          <input
            v-model="searchText"
            class="search-input"
            type="search"
            placeholder="Search snippets..."
          />
        </div>

        <div
          v-if="filteredPastes.length === 0"
          class="empty-state"
        >
          No snippets found.
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
                :key="paste.code"
                class="paste-table-row"
              >
                <td>
                  <p class="paste-title">
                    {{ paste.title }}
                  </p>

                  <span class="paste-code">
                    /p/{{ paste.code }}
                  </span>
                </td>

                <td>
                  {{ paste.language }}
                </td>

                <td>
                  <span
                    :class="
                      paste.visibility === 'Public'
                        ? 'public-badge'
                        : 'private-badge'
                    "
                  >
                    {{ paste.visibility }}
                  </span>
                </td>

                <td>
                  {{ paste.createdAt }}
                </td>

                <td>
                  {{ paste.expiry }}
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
                      @click="deletePaste(paste.code)"
                    >
                      Delete
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

export default {
  name: "Dashboard",

  components: {
    ChartNoAxesCombined,
    Globe2,
    LockKeyhole
  },

  data() {
    return {
      searchText: "",

      pastes: [
        {
          code: "aX9kL",
          title: "Vue login component",
          language: "JavaScript",
          visibility: "Public",
          createdAt: "21 July 2026",
          expiry: "Never"
        },
        {
          code: "bP7mQ",
          title: "SQL database query",
          language: "SQL",
          visibility: "Private",
          createdAt: "20 July 2026",
          expiry: "1 week"
        },
        {
          code: "cT4zR",
          title: "ASP.NET API example",
          language: "C#",
          visibility: "Public",
          createdAt: "18 July 2026",
          expiry: "1 day"
        },
        {
          code: "cT4zR",
          title: "ASP.NET API example",
          language: "C#",
          visibility: "Public",
          createdAt: "18 July 2026",
          expiry: "1 day"
        }
      ]
    };
  },

  computed: {
    filteredPastes() {
      const keyword = this.searchText.trim().toLowerCase();

      if (!keyword) {
        return this.pastes;
      }

      return this.pastes.filter((paste) => {
        return (
          paste.title.toLowerCase().includes(keyword) ||
          paste.language.toLowerCase().includes(keyword) ||
          paste.code.toLowerCase().includes(keyword)
        );
      });
    },

    publicPasteCount() {
      return this.pastes.filter(
        (paste) => paste.visibility === "Public"
      ).length;
    },

    privatePasteCount() {
      return this.pastes.filter(
        (paste) => paste.visibility === "Private"
      ).length;
    }
  },

  methods: {
    viewPaste(code) {
      this.$router.push(`/paste/${code}`);
    },

    deletePaste(code) {
      const confirmed = window.confirm(
        "Are you sure you want to delete this paste?"
      );

      if (!confirmed) {
        return;
      }

      this.pastes = this.pastes.filter(
        (paste) => paste.code !== code
      );
    }
  }
};
</script>