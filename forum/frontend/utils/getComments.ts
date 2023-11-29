export default async (postId: number) => {
    const response = await genericFetch({
        method: 'GET',
        url: `http://localhost:5112/api/Post/GetComments/${postId}`,
    });

    // If the request was successful, filter out replies to comments to avoid duplicates
    if (response.status === 200) {
        const comments: comment[] = await response.data; // Save the comments to ar variable
        const usedIds = new Set<number>(); // Create a set to store the ids of comments that have replies

        // Loop through all comments
        for (const comment of comments) {
            // If the comment has replies, add the comment id to the set
            if (comment.commentReplies) {
                // Loop through all replies
                for (const reply of comment.commentReplies) {
                    // Add the comment id to the set to not use them again
                    usedIds.add(reply.commentId);
                }
            }
        }

        // Filer out the comment that is already used in a reply
        response.data = comments.filter(comment => !usedIds.has(comment.commentId));
    }

    return response;
};
