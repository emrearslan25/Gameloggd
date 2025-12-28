import { useState } from 'react';
import { Plus, Heart, Edit3, ChevronDown, ThumbsUp, User } from 'lucide-react';

interface GameDetailProps {
  onBack?: () => void;
}

export default function GameDetail({ onBack }: GameDetailProps) {
  const [activeTab, setActiveTab] = useState('friends');
  const [selectedPlatform, setSelectedPlatform] = useState('Select Platform');
  const [isPlatformOpen, setIsPlatformOpen] = useState(false);

  const platforms = ['PlayStation 5', 'PlayStation 4', 'Xbox Series X/S', 'Xbox One', 'PC', 'Nintendo Switch'];

  const gameInfo = {
    title: 'Cyberpunk Legends: Neon Rising',
    year: '2024',
    developer: 'Neon Studios',
    rating: 4.7,
    totalRatings: 2847,
    backdrop: 'https://images.unsplash.com/photo-1531113165519-5eb0816d7e02?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjeWJlcnB1bmslMjBnYW1lJTIwYmFja2Ryb3B8ZW58MXx8fHwxNzY1ODgxNjkxfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral',
    poster: 'https://images.unsplash.com/photo-1579798065595-0d8134bf293c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx2aWRlbyUyMGdhbWUlMjBwb3N0ZXJ8ZW58MXx8fHwxNzY1ODgxNjkxfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral',
  };

  const friends = [
    { id: 1, name: 'Alex Chen', initials: 'AC', color: 'cyan', rating: 5, date: 'Dec 14, 2024', platform: 'PS5' },
    { id: 2, name: 'Sarah Williams', initials: 'SW', color: 'purple', rating: 4, date: 'Dec 12, 2024', platform: 'PC' },
    { id: 3, name: 'Marcus Johnson', initials: 'MJ', color: 'orange', rating: 5, date: 'Dec 10, 2024', platform: 'Xbox Series X' },
    { id: 4, name: 'Emma Davis', initials: 'ED', color: 'green', rating: 4, date: 'Dec 8, 2024', platform: 'PC' },
  ];

  const reviews = [
    {
      id: 1,
      user: 'Alex Chen',
      initials: 'AC',
      color: 'cyan',
      rating: 5,
      date: 'Dec 14, 2024',
      text: 'Absolutely phenomenal! The graphics are stunning and the gameplay is incredibly immersive. Best game I\'ve played this year. The story keeps you engaged from start to finish, and the cyberpunk aesthetic is executed perfectly.',
      likes: 42,
      platform: 'PS5'
    },
    {
      id: 2,
      user: 'Jordan Lee',
      initials: 'JL',
      color: 'purple',
      rating: 4,
      date: 'Dec 11, 2024',
      text: 'Great game with a solid storyline. The open world is massive and there\'s always something to do. A few minor bugs here and there, but nothing game-breaking. The character customization is top-notch.',
      likes: 28,
      platform: 'PC'
    },
    {
      id: 3,
      user: 'Taylor Kim',
      initials: 'TK',
      color: 'orange',
      rating: 5,
      date: 'Dec 9, 2024',
      text: 'This is exactly what I wanted from a cyberpunk game. The neon-soaked city feels alive, and the soundtrack is incredible. Combat is fluid and satisfying. Highly recommend!',
      likes: 35,
      platform: 'PC'
    },
  ];

  const lists = [
    { id: 1, title: 'Best Cyberpunk Games', user: 'Alex Chen', count: 12 },
    { id: 2, title: 'Must-Play 2024 Releases', user: 'Sarah Williams', count: 20 },
    { id: 3, title: 'Open World Masterpieces', user: 'Marcus Johnson', count: 15 },
    { id: 4, title: 'Story-Driven Adventures', user: 'Emma Davis', count: 18 },
    { id: 5, title: 'Games with Amazing Graphics', user: 'Jordan Lee', count: 10 },
  ];

  const getAvatarGradient = (color: string) => {
    const gradients: Record<string, string> = {
      cyan: 'linear-gradient(to bottom right, #00f0ff, #0099ff)',
      purple: 'linear-gradient(to bottom right, #b830ff, #ff30e9)',
      orange: 'linear-gradient(to bottom right, #ff6b00, #ff3030)',
      green: 'linear-gradient(to bottom right, #00ff88, #00f0ff)',
    };
    return gradients[color] || gradients.cyan;
  };

  return (
    <div className="min-h-screen bg-[#0a0a0a] text-white">
      {/* Backdrop Section */}
      <div className="relative h-[400px] overflow-hidden">
        <div className="absolute inset-0">
          <img
            src={gameInfo.backdrop}
            alt={gameInfo.title}
            className="w-full h-full object-cover blur-md scale-110"
          />
          <div className="absolute inset-0 bg-gradient-to-b from-[rgba(10,10,10,0.4)] via-[rgba(10,10,10,0.7)] to-[#0a0a0a]"></div>
        </div>

        {/* Game Info Section */}
        <div className="relative max-w-7xl mx-auto px-6 h-full flex items-end pb-8">
          <div className="flex gap-8 w-full">
            {/* Poster */}
            <div className="flex-shrink-0">
              <div className="w-64 aspect-[2/3] rounded-lg overflow-hidden shadow-2xl border-2 border-white/10">
                <img
                  src={gameInfo.poster}
                  alt={gameInfo.title}
                  className="w-full h-full object-cover"
                />
              </div>
            </div>

            {/* Info Block */}
            <div className="flex-1 flex flex-col justify-end pb-4">
              <h1 className="mb-3">{gameInfo.title}</h1>
              <div className="flex items-center gap-6 mb-4 flex-wrap">
                <span className="text-[#a0a0a0]">{gameInfo.year}</span>
                <span className="text-[#a0a0a0]">•</span>
                <span className="text-[#a0a0a0]">{gameInfo.developer}</span>
              </div>

              {/* Star Rating */}
              <div className="flex items-center gap-3 mb-6">
                <div className="flex items-center gap-1">
                  {[...Array(5)].map((_, i) => (
                    <span
                      key={i}
                      className={i < Math.floor(gameInfo.rating) ? 'text-[#00f0ff]' : 'text-[#a0a0a0]'}
                    >
                      ★
                    </span>
                  ))}
                </div>
                <span className="text-[#00f0ff]">{gameInfo.rating}</span>
                <span className="text-[#666666]">({gameInfo.totalRatings} ratings)</span>
              </div>

              {/* Action Bar */}
              <div className="flex items-center gap-4 flex-wrap">
                <button className="px-6 py-3 bg-gradient-to-r from-[#00f0ff] to-[#b830ff] rounded-full flex items-center gap-2 hover:shadow-[0_0_20px_rgba(0,240,255,0.5)] transition-shadow">
                  <Plus className="w-5 h-5" />
                  Log
                </button>

                <button className="px-6 py-3 bg-[#151515] border border-white/10 rounded-full flex items-center gap-2 hover:border-[#00f0ff] transition-colors">
                  <Heart className="w-5 h-5" />
                  Add to Wishlist
                </button>

                <button className="px-6 py-3 bg-[#151515] border border-white/10 rounded-full flex items-center gap-2 hover:border-[#00f0ff] transition-colors">
                  <Edit3 className="w-5 h-5" />
                  Review
                </button>

                {/* Platform Dropdown */}
                <div className="relative">
                  <button
                    onClick={() => setIsPlatformOpen(!isPlatformOpen)}
                    className="px-6 py-3 bg-[#151515] border border-white/10 rounded-full flex items-center gap-2 hover:border-[#00f0ff] transition-colors"
                  >
                    <span>{selectedPlatform}</span>
                    <ChevronDown className={`w-4 h-4 transition-transform ${isPlatformOpen ? 'rotate-180' : ''}`} />
                  </button>

                  {isPlatformOpen && (
                    <div className="absolute top-full mt-2 w-56 bg-[#151515] border border-white/10 rounded-lg shadow-xl z-50 overflow-hidden">
                      {platforms.map((platform) => (
                        <button
                          key={platform}
                          onClick={() => {
                            setSelectedPlatform(platform);
                            setIsPlatformOpen(false);
                          }}
                          className="w-full px-4 py-3 text-left hover:bg-[rgba(0,240,255,0.1)] hover:text-[#00f0ff] transition-colors border-b border-white/5 last:border-b-0"
                        >
                          {platform}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tabbed Content Section */}
      <div className="max-w-7xl mx-auto px-6 py-12">
        {/* Tabs */}
        <div className="border-b border-white/10 mb-8">
          <div className="flex gap-8">
            <button
              onClick={() => setActiveTab('friends')}
              className={`pb-4 transition-colors relative ${
                activeTab === 'friends' ? 'text-[#00f0ff]' : 'text-[#a0a0a0] hover:text-white'
              }`}
            >
              Friends who played this
              {activeTab === 'friends' && (
                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-gradient-to-r from-[#00f0ff] to-[#b830ff]"></div>
              )}
            </button>

            <button
              onClick={() => setActiveTab('reviews')}
              className={`pb-4 transition-colors relative ${
                activeTab === 'reviews' ? 'text-[#00f0ff]' : 'text-[#a0a0a0] hover:text-white'
              }`}
            >
              Popular Reviews
              {activeTab === 'reviews' && (
                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-gradient-to-r from-[#00f0ff] to-[#b830ff]"></div>
              )}
            </button>

            <button
              onClick={() => setActiveTab('lists')}
              className={`pb-4 transition-colors relative ${
                activeTab === 'lists' ? 'text-[#00f0ff]' : 'text-[#a0a0a0] hover:text-white'
              }`}
            >
              Lists including this game
              {activeTab === 'lists' && (
                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-gradient-to-r from-[#00f0ff] to-[#b830ff]"></div>
              )}
            </button>
          </div>
        </div>

        {/* Tab Content */}
        <div>
          {/* Friends Tab */}
          {activeTab === 'friends' && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {friends.map((friend) => (
                <div
                  key={friend.id}
                  className="bg-[#151515] border border-white/10 rounded-xl p-5 flex items-center gap-4 hover:border-[rgba(0,240,255,0.5)] transition-colors"
                >
                  <div
                    className="w-14 h-14 rounded-full flex items-center justify-center flex-shrink-0 text-white"
                    style={{ background: getAvatarGradient(friend.color) }}
                  >
                    {friend.initials}
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="mb-1">{friend.name}</h3>
                    <div className="flex items-center gap-2 mb-1">
                      <div className="flex items-center gap-1">
                        {[...Array(5)].map((_, i) => (
                          <span
                            key={i}
                            className={`text-sm ${i < friend.rating ? 'text-[#00f0ff]' : 'text-[#a0a0a0]'}`}
                          >
                            ★
                          </span>
                        ))}
                      </div>
                    </div>
                    <div className="flex items-center gap-2 text-sm text-[#a0a0a0]">
                      <span>{friend.platform}</span>
                      <span>•</span>
                      <span>{friend.date}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Reviews Tab */}
          {activeTab === 'reviews' && (
            <div className="flex flex-col gap-6">
              {reviews.map((review) => (
                <div
                  key={review.id}
                  className="bg-[#151515] border border-white/10 rounded-xl p-6 hover:border-[rgba(0,240,255,0.5)] transition-colors"
                >
                  <div className="flex items-start gap-4 mb-4">
                    <div
                      className="w-12 h-12 rounded-full flex items-center justify-center flex-shrink-0 text-white"
                      style={{ background: getAvatarGradient(review.color) }}
                    >
                      {review.initials}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between mb-2 flex-wrap gap-2">
                        <h3>{review.user}</h3>
                        <div className="flex items-center gap-2 text-sm text-[#a0a0a0]">
                          <span>{review.platform}</span>
                          <span>•</span>
                          <span>{review.date}</span>
                        </div>
                      </div>
                      <div className="flex items-center gap-1 mb-3">
                        {[...Array(5)].map((_, i) => (
                          <span
                            key={i}
                            className={i < review.rating ? 'text-[#00f0ff]' : 'text-[#a0a0a0]'}
                          >
                            ★
                          </span>
                        ))}
                      </div>
                    </div>
                  </div>
                  <p className="text-[#a0a0a0] mb-4">{review.text}</p>
                  <button className="flex items-center gap-2 text-[#a0a0a0] hover:text-[#00f0ff] transition-colors">
                    <ThumbsUp className="w-4 h-4" />
                    <span>{review.likes}</span>
                  </button>
                </div>
              ))}
            </div>
          )}

          {/* Lists Tab */}
          {activeTab === 'lists' && (
            <div className="flex flex-col gap-3">
              {lists.map((list) => (
                <div
                  key={list.id}
                  className="bg-[#151515] border border-white/10 rounded-xl p-5 flex items-center justify-between hover:border-[rgba(0,240,255,0.5)] transition-colors cursor-pointer group"
                >
                  <div className="flex-1">
                    <h3 className="mb-1 group-hover:text-[#00f0ff] transition-colors">{list.title}</h3>
                    <div className="flex items-center gap-2 text-sm text-[#a0a0a0]">
                      <span>by {list.user}</span>
                      <span>•</span>
                      <span>{list.count} games</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 text-[#a0a0a0] group-hover:text-[#00f0ff] transition-colors">
                    <span className="text-sm">View List</span>
                    <ChevronDown className="w-4 h-4 -rotate-90" />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Back button for demo */}
      {onBack && (
        <div className="fixed bottom-8 left-8">
          <button
            onClick={onBack}
            className="px-6 py-3 bg-[#151515] border border-white/10 rounded-full hover:border-[#00f0ff] transition-colors"
          >
            ← Back to Home
          </button>
        </div>
      )}
    </div>
  );
}
