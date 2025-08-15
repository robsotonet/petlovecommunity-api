using PetLoveCommunity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.Domain.Entities;

public class Post : BaseEntity
{
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public PostType Type { get; set; } = PostType.Text;

    [Required]
    public PostStatus Status { get; set; } = PostStatus.Active;

    [Required]
    public int LikesCount { get; set; } = 0;

    [Required]
    public int CommentsCount { get; set; } = 0;

    [Required]
    public int SharesCount { get; set; } = 0;

    [Required]
    public bool IsPublic { get; set; } = true;

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [StringLength(255)]
    public string? VideoUrl { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; }

    public DateTime? ScheduledAt { get; set; }

    // Foreign key
    [Required]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public virtual ICollection<PostShare> Shares { get; set; } = new List<PostShare>();
    public virtual ICollection<PostPhoto> Photos { get; set; } = new List<PostPhoto>();
}

public class PostComment : BaseEntity
{
    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public CommentStatus Status { get; set; } = CommentStatus.Active;

    [Required]
    public int LikesCount { get; set; } = 0;

    // Foreign keys
    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Guid? ParentCommentId { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual PostComment? ParentComment { get; set; }
    public virtual ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
    public virtual ICollection<PostCommentLike> Likes { get; set; } = new List<PostCommentLike>();
}

public class PostLike : BaseEntity
{
    [Required]
    public PostLikeType Type { get; set; } = PostLikeType.Like;

    // Foreign keys
    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class PostCommentLike : BaseEntity
{
    [Required]
    public PostLikeType Type { get; set; } = PostLikeType.Like;

    // Foreign keys
    [Required]
    public Guid CommentId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual PostComment Comment { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class PostShare : BaseEntity
{
    [StringLength(500)]
    public string? Comment { get; set; }

    [Required]
    public ShareType Type { get; set; } = ShareType.Internal;

    // Foreign keys
    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class PostPhoto : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string PhotoUrl { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Caption { get; set; }

    [Required]
    public int DisplayOrder { get; set; } = 0;

    [Required]
    public bool IsPrimary { get; set; } = false;

    // Foreign key
    [Required]
    public Guid PostId { get; set; }

    // Navigation property
    public virtual Post Post { get; set; } = null!;
}

public class UserFollowing : BaseEntity
{
    [Required]
    public Guid FollowerId { get; set; }

    [Required]
    public Guid FollowingId { get; set; }

    [Required]
    public FollowStatus Status { get; set; } = FollowStatus.Following;

    // Navigation properties
    public virtual User Follower { get; set; } = null!;
    public virtual User Following { get; set; } = null!;
}

public class Notification : BaseEntity
{
    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public bool IsRead { get; set; } = false;

    [Required]
    public bool IsEmailSent { get; set; } = false;

    [StringLength(255)]
    public string? ActionUrl { get; set; }

    public DateTime? ReadAt { get; set; }

    // Foreign keys
    [Required]
    public Guid UserId { get; set; }

    public Guid? RelatedUserId { get; set; }

    public Guid? RelatedEntityId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User? RelatedUser { get; set; }
}

public enum PostType
{
    Text = 0,
    Photo = 1,
    Video = 2,
    Link = 3,
    Poll = 4
}

public enum PostStatus
{
    Active = 0,
    Hidden = 1,
    Deleted = 2,
    Flagged = 3,
    Scheduled = 4
}

public enum CommentStatus
{
    Active = 0,
    Hidden = 1,
    Deleted = 2,
    Flagged = 3
}

public enum PostLikeType
{
    Like = 0,
    Love = 1,
    Laugh = 2,
    Wow = 3,
    Sad = 4,
    Angry = 5
}

public enum ShareType
{
    Internal = 0,
    External = 1
}

public enum FollowStatus
{
    Following = 0,
    Blocked = 1,
    Muted = 2
}

public enum NotificationType
{
    Like = 0,
    Comment = 1,
    Follow = 2,
    Mention = 3,
    AdoptionApplication = 4,
    EventRsvp = 5,
    BookingConfirmation = 6,
    BookingCancellation = 7,
    ReviewReceived = 8,
    System = 9
}