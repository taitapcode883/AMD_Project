<template>
    <div
        v-if="show"
        class="notification"
        :class="type === 'success'
        ? 'notification-success'
        : 'notification-error'"
    >
        <span class="notification-icon">
        {{ type === "success" ? "✓" : "!" }}
        </span>

        <div class="notification-content">
        <strong class="notification-title">
            {{ title }}
        </strong>

        <p class="notification-message">
            {{ message }}
        </p>
        </div>

        <button
        type="button"
        class="notification-close"
        @click="$emit('close')"
        >
        ×
        </button>
    </div>
    </template>

    <script>
    export default {
    name: "Notification",

    props: {
        show: {
        type: Boolean,
        default: false
        },

        type: {
        type: String,
        default: "success"
        },

        title: {
        type: String,
        default: "Success"
        },

        message: {
        type: String,
        default: ""
        }
    },

    emits: ["close"]
    };
    </script>