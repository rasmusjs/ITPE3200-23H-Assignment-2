<script setup lang="ts">
  import { useDisplay } from 'vuetify/lib/framework.mjs';
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '~/constants';

  useHead({
    title: 'BracketBros',
  });

  const display = useDisplay();

  // Computed property to determine the number of columns for categories based on screen size
  const numberOfCategoryCols = computed(() => {
    return display.mdAndUp.value ? 4 : display.smAndUp.value ? 6 : 12;
  });

  // Reactive references for storing available categories and tags
  const availableCategories = ref<category[]>([]);
  const availableTags = ref<tag[]>([]);

  // Lifecycle hook for fetching categories and tags on component mount
  onMounted(async () => {
    // Fetching all categories and sorting them by name
    const getAllCategories_data = await getAllCategories();
    if (getAllCategories_data) {
      availableCategories.value = getAllCategories_data.sort(
        (a: category, b: category) => a.name.localeCompare(b.name)
      );
    } else {
      // Displaying an error toast if fetching categories fails
      toast.error(
        'Error fetching categories from the database.',
        defaultToastOptions.error
      );
    }

    // Fetching all tags and sorting them by name
    const getAllTags_response = await getAllTags();
    if (getAllTags_response.data) {
      // @ts-ignore
      availableTags.value = getAllTags_response.data.sort((a: tag, b: tag) =>
        a.name.localeCompare(b.name)
      );
    } else {
      // Displaying an error toast if fetching tags fails
      toast.error(
        'Error fetching tags from the database.',
        defaultToastOptions.error
      );
    }
  });
</script>

<template>
  <div
    style="max-width: 1000px"
    class="d-flex flex-column w-100 mx-auto px-4 py-16"
  >
    <div class="d-flex flex-column ga-6">
      <h1 class="text-h4 font-weight-bold">
        Introducing BracketBros:<br />Your Go-To Platform for All Things
        Programming!
      </h1>

      <ul class="text-h6">
        <li>
          💬 Dive deep into discussions on languages, frameworks, and tools.
        </li>
        <li>📂 Share your latest projects, code snippets, or challenges.</li>
        <li>
          👥 Collaborate with fellow developers to solve problems and learn new
          techniques.
        </li>
        <li>📚 Curated content to keep you updated on industry trends.</li>
      </ul>

      <p class="text-h6 font-weight-bold">
        Join the BracketBros community now and elevate your coding game!
      </p>
    </div>

    <v-divider class="my-12"></v-divider>

    <h2 class="mb-6 text-h4 font-weight-bold">Categories</h2>
    <v-row>
      <v-col
        v-for="category in availableCategories"
        :cols="numberOfCategoryCols"
      >
        <category-card
          :key="category.categoryId"
          :category="category"
        ></category-card>
      </v-col>
    </v-row>

    <v-divider class="my-12"></v-divider>

    <h2 class="mb-6 text-h4 font-weight-bold">Tags</h2>
    <div class="d-flex flex-wrap ga-2">
      <v-hover v-for="tag in availableTags" :key="tag.tagId">
        <template v-slot:default="{ isHovering, props }">
          <v-chip
            v-bind="props"
            size="x-large"
            :to="`/posts?tagId=${tag.tagId}`"
            class="tag-chip"
            :class="isHovering ? 'hover' : ''"
          >
            {{ tag.name }}
          </v-chip>
        </template>
      </v-hover>
    </div>
  </div>
</template>

<style scoped lang="scss">
  ul {
    list-style: none;
  }

  .tag-chip {
    transition: transform 100ms ease;
    &.hover {
      transform: scale(1.075);
    }
  }
</style>
