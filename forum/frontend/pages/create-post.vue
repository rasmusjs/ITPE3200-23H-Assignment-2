<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '@/constants';

  useHead({
    title: 'Create post - BracketBros',
  });

  const router = useRouter();

  // Reactive references for available categories and tags for a post
  const availableCategories = ref<category[]>([]);
  const availableTags = ref<tag[]>([]);

  // Reactive variables for form state and inputs
  const form = ref(false);
  const title = ref('');
  const selectedCategoryId = ref<number>();
  const selectedTagIds = ref<number[]>([]);
  const content = ref('');

  // Reactive variables for loading states of categories and tags
  const availableCategories_isLoading = ref(false);
  const availableTags_isLoading = ref(false);
  const createPost_isLoading = ref(false);

  // Validation rules for form fields
  const rules = {
    required: (value: any) => {
      if (typeof value === 'string') {
        return value.trim().length > 0 || 'Field is required';
      }
      if (Array.isArray(value)) {
        return value.length > 0 || 'Field is required';
      }
      return !!value || 'Field is required';
    },
    title: (value: string) => {
      const titlePattern = /^[0-9a-zA-ZæøåÆØÅ \-\/':?.!#@$%&*()]{2,64}$/;
      return (
        titlePattern.test(value) ||
        "The title can only contain numbers, letters, or characters -:?.!,'@#$%&*(), and must be between 2 to 64 characters."
      );
    },
    content: (value: string) => {
      value = value.trimEnd();
      return (
        (value.length >= 2 && value.length <= 4096) ||
        'The content must be between 2 to 4096 characters.'
      );
    },
  };

  // Asynchronous function to handle post creation
  const register = async () => {
    createPost_isLoading.value = true;

    // Constructing the post data
    const post: createPostBody = {
      Title: title.value,
      // @ts-ignore - CategoryId is handled by Vuetify validation
      CategoryId: selectedCategoryId.value,
      TagsId: selectedTagIds.value,
      Content: content.value,
    };

    // Sending the create post request
    const response = await createPost(post);

    // Handling the response
    if (response.data) {
      await router.push(`/post/${response.data}`);
    } else {
      toast.error(
        'Unexpected error when trying creating post, please try again later.',
        defaultToastOptions.error
      );
    }

    // Resetting the loading state
    createPost_isLoading.value = false;
  };

  // Lifecycle hook for initial setup
  onMounted(async () => {
    await checkLoginAndReroute(); // Ensuring the user is logged in

    // Fetching available categories and tags for the post form
    availableCategories_isLoading.value = true;
    availableTags_isLoading.value = true;

    const categoriesData = await getAllCategories();
    if (categoriesData) {
      availableCategories.value = categoriesData.sort(
        (a: category, b: category) => a.name.localeCompare(b.name)
      );
    } else {
      toast.error(
        'Error fetching categories from the database.',
        defaultToastOptions.error
      );
    }
    availableCategories_isLoading.value = false;

    const { data: tagsData } = await getAllTags();
    if (tagsData) {
      availableTags.value = tagsData.sort((a: tag, b: tag) =>
        a.name.localeCompare(b.name)
      );
    } else {
      toast.error(
        'Error fetching tags from the database.',
        defaultToastOptions.error
      );
    }
    availableTags_isLoading.value = false;
  });
</script>

<template>
  <nuxt-layout name="centered-content">
    <v-sheet class="pa-12 rounded-lg elevation-4">
      <h1 class="d-flex justify-center align-center mb-8 text-h5">
        Create Post
        <v-icon
          icon="fa:fa-solid fa-pen-to-square"
          size="x-small"
          class="ml-4"
        ></v-icon>
      </h1>

      <v-form v-model="form" @submit.prevent="register">
        <v-text-field
          label="Title"
          v-model="title"
          variant="outlined"
          :rules="[rules.required, rules.title]"
          class="mb-3"
        ></v-text-field>

        <v-select
          label="Category"
          :items="availableCategories"
          :item-title="(category: category) => category.name"
          :item-value="(category: category) => category.categoryId"
          v-model="selectedCategoryId"
          :rules="[rules.required]"
          variant="outlined"
          class="mb-3"
        >
        </v-select>

        <v-select
          label="Tags"
          :items="availableTags"
          :item-title="(tag: tag) => tag.name"
          :item-value="(tag: tag) => tag.tagId"
          v-model="selectedTagIds"
          :rules="[rules.required]"
          multiple
          chips
          variant="outlined"
          class="mb-3"
        >
        </v-select>

        <v-textarea
          label="Content"
          v-model="content"
          variant="outlined"
          :rules="[rules.required, rules.content]"
          :counter="4096"
          class="mb-3"
        ></v-textarea>

        <v-btn
          type="submit"
          size="x-large"
          variant="flat"
          color="primary"
          :disabled="!form"
          :loading="createPost_isLoading"
          block
          class="text-body-1"
        >
          Publish
          <template v-slot:append>
            <v-icon icon="fa:fa-regular fa-paper-plane" size="small"></v-icon>
          </template>
        </v-btn>
      </v-form>
    </v-sheet>
  </nuxt-layout>
</template>
