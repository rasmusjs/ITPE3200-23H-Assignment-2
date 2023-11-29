<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '~/constants';

  const tabTitle = ref('BracketBros');

  useHead({
    title: tabTitle,
  });

  const router = useRouter();
  const route = useRoute();

  // Reactive reference to hold the post data
  const post = ref<post | null>(null);

  // Reactive arrays to store available categories and tags
  const availableCategories = ref<category[]>([]);
  const availableTags = ref<tag[]>([]);

  // Reactive variables for form data
  const form = ref(false);
  const title = ref('');
  const selectedCategoryId = ref<number>();
  const selectedTagIds = ref<number[]>([]);
  const content = ref('');

  // Reactive variables to indicate loading states
  const availableCategories_isLoading = ref(false);
  const availableTags_isLoading = ref(false);
  const editPost_isLoading = ref(false);

  // Watch for changes in the post data to update the tab title
  watch(
    post,
    (newValue) => {
      if (newValue?.title) {
        tabTitle.value = `Edit ${newValue.title} - BracketBros`;
      }
    },
    { immediate: true }
  );

  // Mounted hook to fetch post data and populate the form
  onMounted(async () => {
    const postId = Number(route.params.id);

    // Fetch post data if postId is valid
    if (!isNaN(postId)) {
      const { data: postData, error: postError } = await getPostById(postId);
      if (postError) {
        console.error('Error fetching post:', postError);
        toast.error('Error fetching post', defaultToastOptions.error);
      } else {
        post.value = postData;

        // Populate the form with fetched data
        title.value = postData.title;
        selectedCategoryId.value = postData.category.categoryId;
        selectedTagIds.value = postData.tags.map(
          (t: { tagId: number }) => t.tagId
        );
        content.value = postData.content;
      }
    } else {
      console.error('Invalid Post ID');
      toast.error('Invalid Post ID', defaultToastOptions.error);
    }
  });

  // Validation rules for the form fields
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

  // Function to save the edited post
  const save = async () => {
    editPost_isLoading.value = true;

    const post: editPostBody = {
      id: Number(route.params.id),
      Title: title.value,
      // @ts-ignore - CategoryId is handled by Vuetify validation
      CategoryId: selectedCategoryId.value,
      TagsId: selectedTagIds.value,
      Content: content.value,
    };

    // Update the post
    const response = await updatePost(post);

    // Navigate to the updated post or show error toast
    if (response && response.data) {
      await router.push(`/post/${response.data}`);
    } else {
      toast.error(
        'Unexpected error when trying creating post, please try again later.',
        defaultToastOptions.error
      );
    }

    editPost_isLoading.value = false;
  };

  // Another mounted hook for additional data fetching
  onMounted(async () => {
    await checkLoginAndReroute();

    // Fetch and populate categories and tags
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
      // @ts-ignore
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
        Edit Post
        <v-icon
          icon="fa:fa-solid fa-pen-to-square"
          size="x-small"
          class="ml-4"
        ></v-icon>
      </h1>

      <v-form v-model="form" @submit.prevent="save">
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
          :loading="editPost_isLoading"
          block
          class="text-body-1"
        >
          Save
          <template v-slot:append>
            <v-icon icon="fa:fa-regular fa-save" size="small"></v-icon>
          </template>
        </v-btn>
      </v-form>
    </v-sheet>
  </nuxt-layout>
</template>
