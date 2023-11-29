<script setup lang="ts">
  defineProps<{
    category: category;
  }>();
</script>

<template>
  <v-hover>
    <template v-slot:default="{ isHovering, props }">
      <v-card
        v-bind="props"
        :color="category.color"
        class="category-card w-100 px-2 pb-2 rounded-lg"
        :class="isHovering ? 'hover' : ''"
        :to="`/posts?categoryId=${category.categoryId}`"
      >
        <v-card-item class="text-center">
          <v-card-title>{{ category.name }}</v-card-title>
        </v-card-item>
        <v-img :src="category.url" class="category-image rounded-lg"></v-img>
      </v-card>
    </template>
  </v-hover>
</template>

<style scoped lang="scss">
  .category-card {
    transition: transform 200ms ease;
    &.hover {
      transform: scale(1.05);
    }
  }
</style>
