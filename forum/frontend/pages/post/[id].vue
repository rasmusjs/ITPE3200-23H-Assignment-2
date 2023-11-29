<script setup lang="ts">
  import { toast } from 'vue3-toastify';
  import { defaultToastOptions } from '@/constants';

  const title = ref('BracketBros');

  useHead({
    title: title,
  });

  const route = useRoute();

  // Reactive references for the post and its comments
  const post = ref<post | null>(null);
  const comments = ref<comment[] | null>(null);

  // Asynchronous function to update comments from the server
  const updateComments = async () => {
    if (post.value) {
      // Fetching comments for the current post
      const { data, error, status } = await getComments(post.value?.id);
      if (error && status !== 404) {
        // Handling errors other than 'not found'
        console.error('Error fetching comments:', error);
        toast.error('Error fetching comments', defaultToastOptions.error);
      } else {
        // Updating the comments reactive reference
        comments.value = data;
      }
    }
  };

  // Function to handle the addition of new comments
  const handleCommentAdded = () => {
    updateComments(); // Update comments list
    if (post.value) {
      // Incrementing the total comments count
      post.value.totalComments++;
    }
  };

  // Function to handle comment edits
  const handleCommentEdited = () => {
    updateComments(); // Refresh the comments list
  };

  // Function to handle comment deletion
  const handleDeletedComment = (deletedCommentId: number) => {
    if (comments.value) {
      // Removing the deleted comment from the comments array
      comments.value = comments.value.filter(
        (comment) => comment.commentId !== deletedCommentId
      );
    }
    if (post.value) {
      // Decrementing the total comments count
      post.value.totalComments--;
    }
  };

  // Watching for changes to the post data
  watch(
    post,
    (newValue) => {
      // Updating the page title when the post data changes
      if (newValue?.title) {
        title.value = `${newValue.title} - BracketBros`;
      }
    },
    { immediate: true } // Immediate flag to run the effect on initialization
  );

  // Mounted lifecycle hook to fetch post and comments data
  onMounted(async () => {
    // Getting the post ID from the route parameters
    const postId = Number(route.params.id);

    if (!isNaN(postId)) {
      // Fetching the post data if the ID is valid
      const { data: postData, error: postError } = await getPostById(postId);
      if (postError) {
        // Handling fetch error
        console.error('Error fetching post:', postError);
        toast.error('Error fetching post', defaultToastOptions.error);
      } else {
        // Setting the fetched post data and updating comments
        post.value = postData;
        await updateComments();
      }
    } else {
      // Handling invalid post ID scenario
      console.error('Invalid Post ID');
      toast.error('Invalid Post ID', defaultToastOptions.error);
    }
  });
</script>

<template>
  <nuxt-layout name="centered-content">
    <post-component
      v-if="post"
      :post="post"
      :expandContent="true"
      :preventHighlighting="true"
      @comment-added="handleCommentAdded"
    ></post-component>
    <div class="d-flex flex-column w-100 my-4" style="max-width: 700px">
      <comment-component
        v-for="comment in comments"
        :key="comment.commentId"
        :comment="comment"
        class="w-100 mb-4"
        @comment-reply-added="handleCommentAdded"
        @comment-edited="handleCommentEdited"
        @commentDeleted="handleDeletedComment(comment.commentId)"
      ></comment-component>
    </div>
  </nuxt-layout>
</template>
