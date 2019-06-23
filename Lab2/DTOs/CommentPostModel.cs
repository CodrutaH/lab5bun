using Lab2.Models;

namespace Lab2.Controllers
{
    public class CommentPostModel
    {
        public string Text { get; set; }
        public bool Important { get; set; }



        public static Comment ToComment(CommentPostModel comment)
        {
            return new Comment
            {
                Text = comment.Text,
                Important = comment.Important,


            };
        }
    }
}