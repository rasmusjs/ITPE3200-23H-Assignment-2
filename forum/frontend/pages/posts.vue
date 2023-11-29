<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '@/constants';

  useHead({
    title: 'Browse posts - BracketBros',
  });

  const userActivity = useUserActivity();

  const allPosts = useAllPosts();
  const numberOfDisplayedPosts = ref(5);

  // Reactive variables for managing filter sidebar state and filter inputs
  const showFilterSidebar = ref(false);
  const search = ref('');
  const liked = ref(false);
  const saved = ref(false);
  const createdByMe = ref(false);
  const categories = ref<{ category: category; selected: boolean }[]>([]);
  const tags = ref<{ tag: tag; selected: boolean }[]>([]);

  // Computed property to check if any filter is applied
  const isAnyFilterApplied = computed(() => {
    // Check if search is not empty
    const isSearchApplied = search.value.trim() !== '';

    // Check if liked, saved, or createdByMe is true
    const isUserFilterApplied = liked.value || saved.value || createdByMe.value;

    // Check if any category is selected
    const isAnyCategorySelected = categories.value.some((cat) => cat.selected);

    // Check if any tag is selected
    const isAnyTagSelected = tags.value.some((tag) => tag.selected);

    // Return true if any of these conditions are true
    return (
      isSearchApplied ||
      isUserFilterApplied ||
      isAnyCategorySelected ||
      isAnyTagSelected
    );
  });

  const filteredPosts = computed(() => {
    return allPosts.value.filter((post) => {
      // Convert search query to lowercase for case-insensitive comparison
      const lowerCaseSearch = search.value.toLowerCase();

      // Check if post matches the search query in title, content, category, tags, or username
      const matchesSearch = lowerCaseSearch
        ? post.title.toLowerCase().includes(lowerCaseSearch) ||
          post.content.toLowerCase().includes(lowerCaseSearch) ||
          post.category.name.toLowerCase().includes(lowerCaseSearch) ||
          post.tags.some((tag) =>
            tag.name.toLowerCase().includes(lowerCaseSearch)
          ) ||
          post.user.username.toLowerCase().includes(lowerCaseSearch)
        : true; // If no search query is provided, all posts match

      // Check if any category is selected
      const isAnyCategorySelected = categories.value.some(
        (cat) => cat.selected
      );

      // Check if post matches any of the selected categories
      const matchesCategory = isAnyCategorySelected
        ? categories.value.some(
            (cat) =>
              cat.selected &&
              cat.category.categoryId === post.category.categoryId
          )
        : true; // If no category is selected, all posts match

      // Check if any tag is selected
      const isAnyTagSelected = tags.value.some((tag) => tag.selected);

      // Check if post matches any of the selected tags
      const matchesTag = isAnyTagSelected
        ? post.tags.some((postTag) =>
            tags.value.some(
              (tag) => tag.selected && tag.tag.tagId === postTag.tagId
            )
          )
        : true; // If no tag is selected, all posts match

      // Initialize flags for user-specific filters
      let matchesLiked = true;
      let matchesSaved = true;
      let matchesCreatedByMe = true;

      // Apply user-specific filters (liked, saved, created by me) if user is logged in
      if (userActivity.value && userActivity.value.username) {
        matchesLiked = liked.value
          ? userActivity.value.likedPosts.includes(post.id)
          : true;
        matchesSaved = saved.value
          ? userActivity.value.savedPosts.includes(post.id)
          : true;
        matchesCreatedByMe = createdByMe.value
          ? userActivity.value.posts.includes(post.id)
          : true;
      }

      // Combine all filter conditions
      return (
        matchesSearch &&
        matchesCategory &&
        matchesTag &&
        matchesLiked &&
        matchesSaved &&
        matchesCreatedByMe
      );
    });
  });

  const resetFilters = () => {
    // Reset search string
    search.value = '';

    // Unselect "My interactions"
    liked.value = false;
    saved.value = false;
    createdByMe.value = false;

    // Unselect all categories
    categories.value.forEach((category) => {
      category.selected = false;
    });

    // Unselect all tags
    tags.value.forEach((tag) => {
      tag.selected = false;
    });
  };

  // Function to handle addition of comments to posts
  const handleCommentAdded = (postId: number) => {
    const postIndex = allPosts.value.findIndex((p) => p.id === postId);

    if (postIndex !== -1) {
      const updatedPost = {
        ...allPosts.value[postIndex],
        totalComments: allPosts.value[postIndex].totalComments + 1,
      };

      allPosts.value[postIndex] = updatedPost;
    }
  };

  // Function to check if the user has scrolled near the bottom of the page
  const isNearBottom = () => {
    const scrollY = window.scrollY;
    const visible = document.documentElement.clientHeight;
    const pageHeight = document.documentElement.scrollHeight;
    return visible + scrollY >= pageHeight - 200;
  };

  // Function to handle scroll events for infinite scrolling
  const handleScroll = () => {
    if (
      numberOfDisplayedPosts.value < allPosts.value.length &&
      isNearBottom()
    ) {
      numberOfDisplayedPosts.value += 5;
    }
  };

  const route = useRoute();
  const router = useRouter();

  // Lifecycle hook for fetching categories, tags, and handling query parameters
  onMounted(async () => {
    const getAllCategories_response = await getAllCategories();
    if (getAllCategories_response) {
      categories.value = getAllCategories_response
        .sort((a: category, b: category) => a.name.localeCompare(b.name))
        .map((category: category) => ({
          category: category,
          selected: false,
        }));
    } else {
      toast.error(
        'Error fetching categories from database.',
        defaultToastOptions.error
      );
    }

    const getAllTags_response = await getAllTags();
    if (getAllTags_response.data) {
      tags.value = getAllTags_response.data
        .sort((a: tag, b: tag) => a.name.localeCompare(b.name))
        .map((tag: tag) => ({
          tag: tag,
          selected: false,
        }));
    } else {
      toast.error(
        'Error fetching tags from database.',
        defaultToastOptions.error
      );
    }

    const categoryIdFromUrl = route.query.categoryId;
    const tagIdFromUrl = route.query.tagId;

    if (categoryIdFromUrl) {
      const categoryToSelect = categories.value.find(
        (c) => c.category.categoryId.toString() === categoryIdFromUrl
      );
      if (categoryToSelect) {
        categoryToSelect.selected = true;
      }
    }

    if (tagIdFromUrl) {
      const tagToSelect = tags.value.find(
        (t) => t.tag.tagId.toString() === tagIdFromUrl
      );
      if (tagToSelect) {
        tagToSelect.selected = true;
      }
    }

    if (categoryIdFromUrl || tagIdFromUrl) {
      await router.replace({
        path: route.path,
      });
    }

    window.addEventListener('scroll', handleScroll);
  });

  // Lifecycle hook to remove event listener on component unmount
  onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll);
  });
</script>

<template>
  <v-navigation-drawer rail permanent>
    <v-tooltip text="Filter and search posts" open-delay="500">
      <template v-slot:activator="{ props }">
        <v-list-item
          v-bind="props"
          prepend-icon=""
          :active="showFilterSidebar"
          variant="text"
          class="py-4 text-cyan"
          @click="showFilterSidebar = !showFilterSidebar"
        >
          <template v-slot:prepend>
            <v-badge
              v-if="isAnyFilterApplied"
              dot
              location="bottom end"
              color="orange"
            >
              <v-icon icon="fa:fa-solid fa-filter"></v-icon>
            </v-badge>
            <v-icon v-else icon="fa:fa-solid fa-filter"></v-icon>
          </template>
        </v-list-item>
      </template>
    </v-tooltip>
    <v-tooltip text="Reset filters" open-delay="500">
      <template v-slot:activator="{ props }">
        <v-list-item
          v-bind="props"
          prepend-icon="fa:fa-solid fa-rotate-left"
          variant="text"
          class="py-4 text-cyan"
          :disabled="!isAnyFilterApplied"
          @click="resetFilters"
        ></v-list-item>
      </template>
    </v-tooltip>
  </v-navigation-drawer>

  <v-navigation-drawer v-model="showFilterSidebar" width="320">
    <div class="d-flex flex-column pa-8">
      <v-text-field
        label="Search"
        v-model="search"
        variant="outlined"
        hide-details
      ></v-text-field>

      <v-divider class="my-8"></v-divider>

      <div v-if="!userActivity?.username" class="mb-2 text-caption">
        <v-icon
          icon="fa:fa-solid fa-info-circle"
          size="small"
          class="mr-1"
        ></v-icon>
        Only accessible to logged in users
      </div>
      <v-checkbox
        label="Liked"
        v-model="liked"
        density="compact"
        hide-details
        :disabled="!userActivity?.username"
      ></v-checkbox>
      <v-checkbox
        label="Saved"
        v-model="saved"
        density="compact"
        hide-details
        :disabled="!userActivity?.username"
      ></v-checkbox>
      <v-checkbox
        label="Created by me"
        v-model="createdByMe"
        density="compact"
        hide-details
        :disabled="!userActivity?.username"
      ></v-checkbox>

      <v-divider class="my-8"></v-divider>

      <div>Categories</div>
      <v-checkbox
        v-for="category in categories"
        :key="category.category.categoryId"
        :label="category.category.name"
        v-model="category.selected"
        density="compact"
        hide-details
      ></v-checkbox>

      <v-divider class="my-8"></v-divider>

      <div>Tags</div>
      <v-checkbox
        v-for="tag in tags"
        :key="tag.tag.tagId"
        :label="tag.tag.name"
        v-model="tag.selected"
        density="compact"
        hide-details
      ></v-checkbox>
    </div>
  </v-navigation-drawer>

  <nuxt-layout name="centered-content">
    <post-component
      v-if="filteredPosts.length > 0"
      v-for="post in filteredPosts.slice(0, numberOfDisplayedPosts)"
      :key="post.id"
      :post="post"
      class="mb-8"
      @comment-added="handleCommentAdded(post.id)"
    ></post-component>
    <div v-else-if="allPosts.length > 0" class="text-center">
      No posts match these filters
    </div>
    <div v-else class="text-center">
      No posts available due to error when fetching posts
    </div>
  </nuxt-layout>
</template>
