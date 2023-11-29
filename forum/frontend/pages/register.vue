<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '~/constants';

  useHead({
    title: 'Register - BracketBros',
  });

  const router = useRouter();

  // Reactive variables for form state and inputs
  const form = ref(false);
  const email = ref('');
  const username = ref('');
  const password = ref('');
  const confirmPassword = ref('');
  const rememberMe = ref(false);

  // Reactive variables for showing passwords and loading state
  const showPassword = ref(false);
  const showConfirmPassword = ref(false);
  const isLoading = ref(false);
  const error = ref<null | 'unexpectedError'>(null);

  // Validation rules for form fields
  const rules = {
    required: (value: string) => !!value || 'Field is required',
    username: (value: string) => {
      // Regular expression pattern for validating the username
      const usernamePattern = /^[a-zA-Z0-9]{3,20}$/;
      return usernamePattern.test(value) || 'Please enter a valid username';
    },
    email: (value: string) => {
      // Regular expression pattern for validating the email
      const emailPattern =
        /^([a-zA-Z0-9_\-.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(]?)$/;
      return emailPattern.test(value) || 'Please enter a valid email';
    },
    password: (value: string) => {
      // Regular expression pattern for validating the password
      // Source https://stackoverflow.com/questions/8699033/password-dataannotation-in-asp-net-mvc-3

      const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
      return (
        passwordPattern.test(value) ||
        'Password must contain at least 8 characters, one uppercase, one lowercase, and one number'
      );
    },
    passwordMatch: () =>
      password.value === confirmPassword.value || 'Passwords must match',
  };

  // Watcher to trigger re-validation for confirmPassword when password changes
  watch(password, () => {
    confirmPassword.value; // Accessing confirmPassword value to trigger reactivity
  });

  // Asynchronous function to handle user registration
  const register = async () => {
    isLoading.value = true;

    // Constructing registration data
    const registerData: registerData = {
      email: email.value,
      username: username.value,
      password: password.value,
    };

    // Sending the registration request
    const response = await registerUser(registerData);

    // Handling the registration response
    if (response.data) {
      // Navigating to manage-account page upon successful registration
      await router.replace('/manage-account');
    } else {
      // Displaying error toast if registration fails
      toast.error(
        'Unexpected error when trying to register new account.',
        defaultToastOptions.error
      );
    }

    // Resetting the loading state
    isLoading.value = false;
  };
</script>

<template>
  <nuxt-layout name="login-register">
    <v-form v-model="form" @submit.prevent="register">
      <v-text-field
        label="Email"
        v-model="email"
        variant="outlined"
        :rules="[rules.required, rules.email]"
        class="mb-3"
      ></v-text-field>
      <v-text-field
        label="Username"
        v-model="username"
        variant="outlined"
        :rules="[rules.required, rules.username]"
        class="mb-3"
      ></v-text-field>
      <v-text-field
        label="Password"
        v-model="password"
        :type="showPassword ? 'text' : 'password'"
        variant="outlined"
        :rules="[rules.required, rules.password]"
        class="mb-3"
      >
        <template v-slot:append-inner>
          <v-icon
            :icon="
              showPassword ? 'fa:fa-solid fa-eye-slash' : 'fa:fa-solid fa-eye'
            "
            size="x-small"
            @click="showPassword = !showPassword"
          ></v-icon>
        </template>
      </v-text-field>
      <v-text-field
        label="Confirm password"
        v-model="confirmPassword"
        :type="showConfirmPassword ? 'text' : 'password'"
        variant="outlined"
        :rules="[rules.required, rules.password, rules.passwordMatch]"
      >
        <template v-slot:append-inner>
          <v-icon
            :icon="
              showConfirmPassword
                ? 'fa:fa-solid fa-eye-slash'
                : 'fa:fa-solid fa-eye'
            "
            size="x-small"
            @click="showConfirmPassword = !showConfirmPassword"
          ></v-icon>
        </template>
      </v-text-field>
      <v-checkbox
        label="Remember me"
        v-model="rememberMe"
        hide-details
        class="mb-5"
      ></v-checkbox>
      <v-btn
        type="submit"
        size="x-large"
        variant="tonal"
        block
        class="text-body-1"
        :disabled="!form"
        :loading="isLoading"
      >
        Register
      </v-btn>
    </v-form>
    <nuxt-link to="/login" class="d-block mt-8 text-body-2">
      Already have an account? Log in here.
    </nuxt-link>
  </nuxt-layout>
</template>
