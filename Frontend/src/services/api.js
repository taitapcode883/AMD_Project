const API_URL = import.meta.env.VITE_API_URL || "";

export function getToken() {
  return localStorage.getItem("token");
}

export function saveSession(data) {
  if (data.token) {
    localStorage.setItem("token", data.token);
  }

  if (data.refreshToken) {
    localStorage.setItem(
      "refreshToken",
      data.refreshToken
    );
  }

  if (data.user) {
    localStorage.setItem(
      "user",
      JSON.stringify(data.user)
    );
  }
}

export function clearSession() {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("user");
}

export async function apiRequest(path, options = {}) {
  const token = getToken();

  const headers = {
    "Content-Type": "application/json",
    ...options.headers
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(
    `${API_URL}${path}`,
    {
      ...options,
      headers
    }
  );

  let data = null;

  const contentType =
    response.headers.get("content-type");

  if (
    contentType &&
    contentType.includes("application/json")
  ) {
    data = await response.json();
  } else if (response.status !== 204) {
    data = await response.text();
  }

  if (!response.ok) {
    let message = "Something went wrong.";

    if (typeof data === "string" && data) {
      message = data;
    }

    if (data && typeof data === "object") {
      message =
        data.message ||
        data.title ||
        JSON.stringify(data);
    }

    const error = new Error(
      `${response.status}: ${message}`
    );

    error.status = response.status;
    error.data = data;

    throw error;
  }

  return data;
}