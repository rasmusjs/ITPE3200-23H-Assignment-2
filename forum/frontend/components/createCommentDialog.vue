<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '@/constants';

  // Defining emit events for the component
  const emit = defineEmits(['commentAdded']);

  // Define component props with types and default values
  const props = withDefaults(
    defineProps<{
      type: 'post' | 'comment';
      postId: number;
      parentCommentId?: number | null;
      title: string;
    }>(),
    { parentCommentId: null }
  );

  // Reactive variables for comment dialog state and content
  const showCreateCommentDialog = ref(false);
  const commentDialogForm = ref(false);
  const commentDialogContent = ref('');
  const isLoading = ref(false);

  // Validation rules for comment creation
  const rules = {
    required: (value: string) => {
      return value.trim().length > 0 || 'Field is required';
    },
    content: (value: string) => {
      value = value.trimEnd();
      return (
        (value.length >= 2 && value.length <= 512) ||
        'The content must be between 2 to 512 characters.'
      );
    },
  };

  // Watch for changes in the comment dialog's visibility
  watch(showCreateCommentDialog, (newValue, oldValue) => {
    if (newValue === true) {
      checkLoginAndReroute();
    }
  });

  // Handles the creation of a new comment
  const handleCreateComment = async () => {
    isLoading.value = true;

    // Creating the comment payload
    const comment: createCommentBody = {
      ParentCommentId: props.parentCommentId,
      PostId: props.postId,
      Content: commentDialogContent.value,
    };

    // Sending the create comment request
    const response = await createComment(comment);
    if (!response.error && response.status !== 400) {
      // Reset dialog and emit success event
      showCreateCommentDialog.value = false;
      commentDialogContent.value = '';
      emit('commentAdded');
      toast.success('Comment has been created.', defaultToastOptions.success);
    } else {
      // Handle error case
      toast.error(
        'Error when trying to create comment.',
        defaultToastOptions.error
      );
    }

    // Reset loading state
    isLoading.value = false;
  };
</script>

<template>
  <v-dialog
    v-model="showCreateCommentDialog"
    activator="parent"
    max-width="600"
  >
    <v-card class="px-10 py-6 rounded-lg">
      <div class="text-h6">
        <template v-if="type === 'post'">Comment on post:</template>
        <template v-else>Reply to comment:</template>
        <br />
        "{{ title }}"
      </div>

      <v-card-text class="px-0">
        <v-form v-model="commentDialogForm">
          <v-textarea
            v-model="commentDialogContent"
            variant="outlined"
            :rules="[rules.required, rules.content]"
            counter="512"
          ></v-textarea>
        </v-form>
      </v-card-text>
      <v-card-actions class="px-0">
        <v-btn
          variant="outlined"
          class="text-body-1"
          @click="showCreateCommentDialog = false"
        >
          Cancel
        </v-btn>
        <v-btn
          variant="outlined"
          color="cyan"
          class="text-body-1"
          :disabled="!commentDialogForm"
          :loading="isLoading"
          @click="handleCreateComment"
        >
          Publish
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
