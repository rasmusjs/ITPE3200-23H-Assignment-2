<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '@/constants';

  // Define component emits for various comment actions
  const emit = defineEmits([
    'commentReplyAdded',
    'commentEdited',
    'commentDeleted',
  ]);

  // Define props with defaults for component
  const props = withDefaults(
    defineProps<{
      comment: comment;
      showReplies?: boolean;
      showGoToPost?: boolean;
    }>(),
    { showReplies: true, showGoToPost: false }
  );

  // Reactive variables to track comment status
  const madeByUser = ref(false);
  const isAdmin = ref(false);
  const likedByUser = ref(false);
  const savedByUser = ref(false);

  // Custom state to track user activities
  const userActivity = useUserActivity();

  // Checking and updating the status of the comment based on user activity
  if (userActivity.value) {
    const refUserActivity = userActivity.value;
    refUserActivity.username === props.comment.user.username
      ? (madeByUser.value = true)
      : (madeByUser.value = false);
    refUserActivity.role === 'Admin' ? (isAdmin.value = true) : {};
    refUserActivity.likedComments.includes(props.comment.commentId)
      ? (likedByUser.value = true)
      : (likedByUser.value = false);
    refUserActivity?.savedComments.includes(props.comment.commentId)
      ? (savedByUser.value = true)
      : (savedByUser.value = false);
  }

  // Watch for changes in userActivity and update comment status accordingly
  watchEffect(() => {
    if (userActivity.value?.username) {
      madeByUser.value = userActivity.value.comments.includes(
        props.comment.commentId
      );
      likedByUser.value = userActivity.value.likedComments.includes(
        props.comment.commentId
      );
      savedByUser.value = userActivity.value.savedComments.includes(
        props.comment.commentId
      );
    }
  });

  // Handles liking a comment
  const handleLikeComment = async () => {
    const response = await likeComment(props.comment.commentId);
    if (response && response?.data) {
      if (response.data === 'Liked comment successfully') {
        likedByUser.value = true;
        props.comment.totalLikes++;
      } else {
        likedByUser.value = false;
        props.comment.totalLikes--;
      }
    } else {
      console.log(response);
    }
  };

  // Saves a comment
  const handleSaveComment = () => {
    saveComment(props.comment.commentId);
  };

  // Emit event when a comment reply is added
  const handleCommentAdded = () => {
    emit('commentReplyAdded');
  };

  // Reactive variables for comment editing
  const showEditCommentDialog = ref(false);
  const editCommentForm = ref(false);
  const editCommentContent = ref(props.comment.content);
  const editComment_isLoading = ref(false);

  // Validation rules for comment editing
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

  // Handles editing a comment
  const handleEditComment = async () => {
    editComment_isLoading.value = true;

    const updateCommentBody: updateCommentBody = {
      commentId: props.comment.commentId,
      ParentCommentId: props.comment.parentCommentId,
      PostId: props.comment.postId,
      Content: props.comment.content,
    };

    const response = await updateComment(updateCommentBody);
    if (response) {
      toast.success('Comment has been edited.', defaultToastOptions.success);
      showEditCommentDialog.value = false;
    } else {
      toast.error(
        'Error when trying to edit comment.',
        defaultToastOptions.error
      );
    }

    emit('commentEdited');
    editComment_isLoading.value = false;
  };

  // Reactive variable for comment deletion confirmation
  const showDeleteCommentDialog = ref(false);

  // Handles deleting a comment
  const handleDeleteComment = () => {
    showDeleteCommentDialog.value = false;
    deleteComment(props.comment.commentId);
    emit('commentDeleted');
  };

  const router = useRouter();

  // Navigates to the post associated with the comment
  const goToPost = async () => {
    await router.push(`/post/${props.comment.postId}`);
  };
</script>

<template>
  <div>
    <v-card class="px-1 py-2 rounded-lg elevation-4">
      <div class="d-flex flex-row align-center pl-2">
        <v-hover>
          <template v-slot:default="{ isHovering, props }">
            <v-avatar
              v-bind="props"
              size="12px"
              class="user-avatar mr-2 border"
              :class="isHovering ? 'is-hovering' : ''"
            >
              <v-img
                v-if="comment.user.profilePicture"
                :src="comment.user.profilePicture"
              ></v-img>
              <v-icon
                v-else
                color="primary"
                icon="fa:fa-solid fa-user"
                size="xx-small"
              ></v-icon>
            </v-avatar>
          </template>
        </v-hover>
        <span class="text-caption">
          <span class="mr-1 text-medium-emphasis">
            {{ comment.user.username }}
          </span>
          <span class="text-disabled">
            {{ formatTimeAgo(comment.dateCreated) }}
          </span>
        </span>
        <v-btn
          v-if="showGoToPost"
          icon
          size="x-small"
          variant="plain"
          density="comfortable"
          class="ml-auto mr-1 text-caption"
          @click="goToPost"
        >
          <v-icon icon="fa:fa-solid fa-arrow-right" size="small"></v-icon>
          <v-tooltip activator="parent">Go to post</v-tooltip>
        </v-btn>
      </div>
      <div class="px-2 py-1">
        {{ comment.content }}
      </div>
      <div class="d-flex flex-row align-center justify-space-between px-1">
        <div>
          <v-btn variant="plain" size="x-small" @click="handleLikeComment">
            <template v-slot:prepend>
              <v-icon
                :icon="
                  likedByUser
                    ? 'fa:fa-solid fa-heart'
                    : 'fa:fa-regular fa-heart'
                "
                :color="likedByUser ? 'red' : ''"
              ></v-icon>
            </template>
            {{ formatNumber(comment.totalLikes) }}
          </v-btn>
          <v-btn variant="plain" size="x-small">
            <template v-slot:prepend>
              <v-icon icon="fa:fa-regular fa-comment"></v-icon>
            </template>
            {{ formatNumber(comment.commentReplies.length) }}
            <create-comment-dialog
              type="comment"
              :title="comment.content"
              :post-id="comment.postId"
              :parent-comment-id="comment.commentId"
              @comment-added="handleCommentAdded"
            ></create-comment-dialog>
          </v-btn>
          <v-btn
            @click="handleSaveComment"
            variant="plain"
            icon
            size="x-small"
            density="comfortable"
            class="rounded"
          >
            <v-icon
              :icon="
                savedByUser
                  ? 'fa:fa-solid fa-bookmark'
                  : 'fa:fa-regular fa-bookmark'
              "
              :color="savedByUser ? 'blue' : ''"
              size="small"
            ></v-icon>
          </v-btn>
        </div>
        <div v-if="madeByUser || isAdmin">
          <v-btn
            :disabled="!madeByUser"
            variant="plain"
            icon
            size="x-small"
            density="comfortable"
            class="rounded"
          >
            <v-icon icon="fa:fa-solid fa-pen-to-square" size="small"></v-icon>
            <v-dialog
              v-model="showEditCommentDialog"
              activator="parent"
              max-width="600"
            >
              <v-card class="px-10 py-6 rounded-lg">
                <div class="text-h6">Edit comment:</div>
                <v-divider class="my-2"></v-divider>
                <div class="text-body-1">
                  {{ comment.content }}
                </div>
                <v-divider class="my-2"></v-divider>
                <v-card-text class="px-0">
                  <v-form v-model="editCommentForm">
                    <v-textarea
                      v-model="editCommentContent"
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
                    @click="showEditCommentDialog = false"
                  >
                    Cancel
                  </v-btn>
                  <v-btn
                    variant="outlined"
                    color="cyan"
                    class="text-body-1"
                    :disabled="!editCommentForm"
                    :loading="editComment_isLoading"
                    @click="handleEditComment"
                  >
                    Edit
                  </v-btn>
                </v-card-actions>
              </v-card>
            </v-dialog>
          </v-btn>
          <v-btn
            variant="plain"
            icon
            size="x-small"
            density="comfortable"
            class="rounded"
          >
            <v-icon icon="fa:fa-solid fa-trash-can" size="small"></v-icon>
            <v-dialog
              v-model="showDeleteCommentDialog"
              activator="parent"
              width="auto"
            >
              <v-card class="px-10 py-6 rounded-lg">
                <v-card-item class="px-0">
                  <v-card-title class="text-h5">Delete comment</v-card-title>
                </v-card-item>
                <v-card-text class="px-0">
                  Are you sure you want to permanently delete this comment?
                  <v-divider class="my-2"></v-divider>
                  "{{ comment.content }}"
                  <v-divider class="mt-2"></v-divider>
                </v-card-text>
                <v-card-actions class="px-0">
                  <v-btn
                    variant="outlined"
                    class="text-body-1"
                    @click="showDeleteCommentDialog = false"
                  >
                    No, cancel
                  </v-btn>
                  <v-btn
                    variant="outlined"
                    color="error"
                    class="text-body-1"
                    @click="handleDeleteComment"
                  >
                    Yes, delete comment
                  </v-btn>
                </v-card-actions>
              </v-card>
            </v-dialog>
          </v-btn>
        </div>
      </div>
    </v-card>
    <commentComponent
      v-if="showReplies"
      v-for="reply in comment.commentReplies"
      :key="reply.commentId"
      :comment="reply"
      class="mt-2 ml-6"
    ></commentComponent>
  </div>
</template>

<style scoped lang="scss">
  .user-avatar {
    transition: transform 200ms ease;

    &.is-hovering {
      transform: scale(2.25);
    }
  }
</style>
