(function () {
  const alertMessage = "Swagger has been authorized automatically.";
  const schemeName = "Bearer";

  const applyToken = (token) => {
    if (!window.ui || !window.ui.authActions) {
      return;
    }

    const payload = {
      [schemeName]: {
        schema: {
          type: "http",
          scheme: "bearer",
          bearerFormat: "JWT"
        },
        value: token
      }
    };

    window.ui.authActions.authorize(payload);
    alert(alertMessage);
  };

  const fetchToken = async () => {
    const response = await fetch("/api/auth/refresh-token", {
      method: "GET",
      credentials: "include",
      headers: {
        "Accept": "application/json"
      }
    });

    if (!response.ok) {
      console.warn("Swagger auto-auth token refresh failed.", response.status);
      return;
    }

    const payload = await response.json();
    const token = payload?.token || payload?.value?.token;
    if (!token) {
      console.warn("Swagger auto-auth response did not include a token.");
      return;
    }

    applyToken(token);
  };

  const ensureUiReady = () => {
    if (window.ui && typeof window.ui.preauthorizeApiKey === "function") {
      fetchToken();
      return;
    }

    setTimeout(ensureUiReady, 200);
  };

  if (document.readyState === "complete") {
    ensureUiReady();
  } else {
    window.addEventListener("load", ensureUiReady, { once: true });
  }
})();
