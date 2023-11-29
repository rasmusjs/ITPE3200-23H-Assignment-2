<script setup lang="ts">
  import { useTheme } from 'vuetify/lib/framework.mjs';

  // Accessing the theme object
  const theme = useTheme();

  // Ref to track the 'on-surface-variant' color from the theme
  const onSurfaceVariant = ref(
    theme.current.value.colors['on-surface-variant']
  );

  // Watching for changes in the 'on-surface-variant' theme color
  watch(
    () => theme.current.value.colors['on-surface-variant'],
    (newColor) => {
      onSurfaceVariant.value = newColor; // Update the ref when the color changes
    },
    { immediate: true } // Immediate flag to run on component mount
  );

  // Define props accepted by the component
  defineProps<{ content: string }>();
</script>

<template>
  <div v-html="$mdRenderer.render(content)"></div>
</template>

<style scoped lang="scss">
  :deep() {
    h1,
    h2,
    h3,
    h4,
    h5,
    h6,
    p,
    hr,
    ul,
    pre {
      margin-bottom: 12px;

      &:last-child {
        margin-bottom: 0;
      }
    }

    ul {
      margin-left: 24px;
    }

    pre {
      padding: 10px 12px;
      font-size: 14px;
      white-space: pre-wrap;
      background-color: v-bind(onSurfaceVariant);
      border-radius: 8px;
    }

    :not(pre) code {
      padding: 1px 4px;
      background-color: v-bind(onSurfaceVariant);
      border-radius: 4px;
    }
  }
</style>
