import { useState } from 'react';
import { Calendar, Clock, Users, Star, ChevronLeft } from 'lucide-react';

interface UserProfileProps {
  onBack?: () => void;
}

export default function UserProfile({ onBack }: UserProfileProps) {
  const [activeView, setActiveView] = useState<'activity' | 'diary'>('activity');

  const userStats = {
    name: 'Alex Chen',
    username: '@alexchen',
    bio: 'Indie game enthusiast | RPG lover | Currently playing everything cyberpunk. Always looking for hidden gems and narrative-driven experiences.',
    avatar: 'linear-gradient(to bottom right, #00f0ff, #0099ff)',
    initials: 'AC',
    stats: {
      gamesPlayed: 247,
      hoursLogged: 1842,
      followers: 1284,
      following: 892,
    },
  };

  const favoriteGames = [
    {
      id: 1,
      title: 'Cyber Strike: Revolution',
      rating: 5,
      image: 'https://images.unsplash.com/photo-1629977007398-a17feb6ddf14?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhY3Rpb24lMjBnYW1lJTIwY292ZXJ8ZW58MXx8fHwxNzY1ODgxODYyfDA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral',
    },
    {
      id: 2,
      title: 'Fantasy Realms Online',
      rating: 5,
      image: 'https://images.unsplash.com/photo-1759688168277-185a0c623968?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxycGclMjBmYW50YXN5JTIwZ2FtZXxlbnwxfHx8fDE3NjU3Njc3NjR8MA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral',
    },
    {
      id: 3,
      title: 'Tactical Ops: Shadow War',
      rating: 4.5,
      image: 'https://images.unsplash.com/photo-1759491978401-1dc6f38b6780?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxzaG9vdGVyJTIwZ2FtZXxlbnwxfHx8fDE3NjU4NjE2OTV8MA&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral',
    },
    {
      id: 4,
      title: 'Journey to the Unknown',
      rating: 5,
      image: 'https://images.unsplash.com/photo-1759663174567-5e444de2488c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhZHZlbnR1cmUlMjBnYW1lJTIwYXJ0fGVufDF8fHx8MTc2NTg4MTg2M3ww&ixlib=rb-4.1.0&q=80&w=1080&utm_source=figma&utm_medium=referral',
    },
  ];

  const recentActivity = [
    {
      id: 1,
      game: 'Cyberpunk Legends: Neon Rising',
      rating: 5,
      date: 'Dec 16, 2024',
      platform: 'PS5',
      hours: 45,
      image: 'https://images.unsplash.com/photo-1545579003-84eeef98a485?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjeWJlcnB1bmslMjBnYW1lfGVufDF8fHx8MTc2NTcxNjIzM3ww&ixlib=rb-4.1.0&q=80&w=1080',
    },
    {
      id: 2,
      game: 'Speed Racer X',
      rating: 4,
      date: 'Dec 14, 2024',
      platform: 'PC',
      hours: 12,
      image: 'https://images.unsplash.com/photo-1602940819863-2905852243ad?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxyYWNpbmclMjBnYW1lfGVufDF8fHx8MTc2NTc0NTM2MHww&ixlib=rb-4.1.0&q=80&w=1080',
    },
    {
      id: 3,
      game: 'Adventure Tales',
      rating: 5,
      date: 'Dec 12, 2024',
      platform: 'Xbox Series X',
      hours: 28,
      image: 'https://images.unsplash.com/photo-1759663176932-04c90aeec2b3?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhZHZlbnR1cmUlMjBnYW1lfGVufDF8fHx8MTc2NTc0NzMyM3ww&ixlib=rb-4.1.0&q=80&w=1080',
    },
    {
      id: 4,
      game: 'Fantasy Quest',
      rating: 4,
      date: 'Dec 10, 2024',
      platform: 'PC',
      hours: 56,
      image: 'https://images.unsplash.com/photo-1659480140212-090e6e576080?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmYW50YXN5JTIwZ2FtZXxlbnwxfHx8fDE3NjU3MTI2NjR8MA&ixlib=rb-4.1.0&q=80&w=1080',
    },
    {
      id: 5,
      game: 'Neon Warriors',
      rating: 5,
      date: 'Dec 8, 2024',
      platform: 'PS5',
      hours: 34,
      image: 'https://images.unsplash.com/photo-1761164034378-573f87613427?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHx2aWRlbyUyMGdhbWUlMjBhY3Rpb258ZW58MXx8fHwxNzY1ODA4NTkxfDA&ixlib=rb-4.1.0&q=80&w=1080',
    },
  ];

  const gamesDiary = [
    { game: 'Cyberpunk Legends: Neon Rising', date: 'Dec 16', rating: 5, platform: 'PS5', hours: 45 },
    { game: 'Speed Racer X', date: 'Dec 14', rating: 4, platform: 'PC', hours: 12 },
    { game: 'Adventure Tales', date: 'Dec 12', rating: 5, platform: 'Xbox Series X', hours: 28 },
    { game: 'Fantasy Quest', date: 'Dec 10', rating: 4, platform: 'PC', hours: 56 },
    { game: 'Neon Warriors', date: 'Dec 8', rating: 5, platform: 'PS5', hours: 34 },
    { game: 'Dark Souls: Reborn', date: 'Dec 5', rating: 5, platform: 'PC', hours: 67 },
    { game: 'Pixel Kingdoms', date: 'Dec 2', rating: 4, platform: 'Switch', hours: 15 },
    { game: 'Space Odyssey 2077', date: 'Nov 28', rating: 4, platform: 'PS5', hours: 22 },
    { game: 'Medieval Warriors', date: 'Nov 25', rating: 3, platform: 'Xbox Series X', hours: 18 },
    { game: 'Racing Thunder', date: 'Nov 22', rating: 4, platform: 'PC', hours: 9 },
    { game: 'Horror Mansion', date: 'Nov 18', rating: 5, platform: 'PS5', hours: 14 },
    { game: 'Indie Adventure Deluxe', date: 'Nov 15', rating: 4, platform: 'PC', hours: 8 },
  ];

  return (
    <div className="min-h-screen bg-[#1a1a1a] text-white">
      {/* Header Section */}
      <div className="bg-[#0a0a0a] border-b border-white/10">
        <div className="max-w-6xl mx-auto px-6 py-12">
          <div className="flex flex-col md:flex-row gap-8 items-start md:items-center">
            {/* Avatar */}
            <div
              className="w-32 h-32 rounded-full flex items-center justify-center flex-shrink-0 text-white text-4xl"
              style={{ background: userStats.avatar }}
            >
              {userStats.initials}
            </div>

            {/* User Info */}
            <div className="flex-1">
              <h1 className="mb-1">{userStats.name}</h1>
              <p className="text-[#a0a0a0] mb-4">{userStats.username}</p>
              <p className="text-[#d0d0d0] mb-6 max-w-2xl leading-relaxed">{userStats.bio}</p>

              {/* Stats */}
              <div className="flex flex-wrap gap-6">
                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-lg bg-[rgba(0,240,255,0.1)] border border-[rgba(0,240,255,0.3)] flex items-center justify-center">
                    <Calendar className="w-5 h-5 text-[#00f0ff]" />
                  </div>
                  <div>
                    <div className="text-[#00f0ff]">{userStats.stats.gamesPlayed}</div>
                    <div className="text-sm text-[#a0a0a0]">Games Played</div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-lg bg-[rgba(184,48,255,0.1)] border border-[rgba(184,48,255,0.3)] flex items-center justify-center">
                    <Clock className="w-5 h-5 text-[#b830ff]" />
                  </div>
                  <div>
                    <div className="text-[#b830ff]">{userStats.stats.hoursLogged}</div>
                    <div className="text-sm text-[#a0a0a0]">Hours Logged</div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-lg bg-[rgba(0,240,255,0.1)] border border-[rgba(0,240,255,0.3)] flex items-center justify-center">
                    <Users className="w-5 h-5 text-[#00f0ff]" />
                  </div>
                  <div>
                    <div className="text-[#00f0ff]">{userStats.stats.followers}</div>
                    <div className="text-sm text-[#a0a0a0]">Followers</div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-lg bg-[rgba(184,48,255,0.1)] border border-[rgba(184,48,255,0.3)] flex items-center justify-center">
                    <Users className="w-5 h-5 text-[#b830ff]" />
                  </div>
                  <div>
                    <div className="text-[#b830ff]">{userStats.stats.following}</div>
                    <div className="text-sm text-[#a0a0a0]">Following</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Favorite Games Section */}
      <div className="max-w-6xl mx-auto px-6 py-12">
        <h2 className="mb-6">Top 4 Favorite Games</h2>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-6">
          {favoriteGames.map((game) => (
            <div
              key={game.id}
              className="group cursor-pointer"
            >
              <div className="relative aspect-[2/3] rounded-lg overflow-hidden mb-3 border-2 border-white/10 hover:border-[#00f0ff] transition-colors">
                <img
                  src={game.image}
                  alt={game.title}
                  className="w-full h-full object-cover transition-transform group-hover:scale-105"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/50 to-transparent opacity-0 group-hover:opacity-100 transition-opacity">
                  <div className="absolute bottom-4 left-4 right-4">
                    <div className="flex items-center gap-1 mb-2">
                      {[...Array(5)].map((_, i) => (
                        <Star
                          key={i}
                          className={`w-4 h-4 ${
                            i < Math.floor(game.rating)
                              ? 'fill-[#00f0ff] text-[#00f0ff]'
                              : 'text-[#a0a0a0]'
                          }`}
                        />
                      ))}
                    </div>
                  </div>
                </div>
              </div>
              <h3 className="group-hover:text-[#00f0ff] transition-colors">{game.title}</h3>
            </div>
          ))}
        </div>
      </div>

      {/* Activity Toggle */}
      <div className="max-w-6xl mx-auto px-6 py-8 border-t border-white/10">
        <div className="flex gap-6 border-b border-white/10">
          <button
            onClick={() => setActiveView('activity')}
            className={`pb-4 transition-colors relative ${
              activeView === 'activity' ? 'text-[#00f0ff]' : 'text-[#a0a0a0] hover:text-white'
            }`}
          >
            Recent Activity
            {activeView === 'activity' && (
              <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-gradient-to-r from-[#00f0ff] to-[#b830ff]"></div>
            )}
          </button>

          <button
            onClick={() => setActiveView('diary')}
            className={`pb-4 transition-colors relative ${
              activeView === 'diary' ? 'text-[#00f0ff]' : 'text-[#a0a0a0] hover:text-white'
            }`}
          >
            2024 Diary
            {activeView === 'diary' && (
              <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-gradient-to-r from-[#00f0ff] to-[#b830ff]"></div>
            )}
          </button>
        </div>
      </div>

      {/* Content Area */}
      <div className="max-w-6xl mx-auto px-6 pb-12">
        {activeView === 'activity' ? (
          // Recent Activity Timeline
          <div className="flex flex-col gap-4">
            {recentActivity.map((activity) => (
              <div
                key={activity.id}
                className="bg-[#0a0a0a] border border-white/10 rounded-xl p-5 flex gap-4 hover:border-[rgba(0,240,255,0.5)] transition-colors group"
              >
                <div className="flex-shrink-0 w-24 aspect-[2/3] rounded-lg overflow-hidden border border-white/10">
                  <img
                    src={activity.image}
                    alt={activity.game}
                    className="w-full h-full object-cover transition-transform group-hover:scale-105"
                  />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="mb-2 group-hover:text-[#00f0ff] transition-colors">
                    {activity.game}
                  </h3>
                  <div className="flex items-center gap-1 mb-3">
                    {[...Array(5)].map((_, i) => (
                      <Star
                        key={i}
                        className={`w-4 h-4 ${
                          i < activity.rating
                            ? 'fill-[#00f0ff] text-[#00f0ff]'
                            : 'text-[#a0a0a0]'
                        }`}
                      />
                    ))}
                  </div>
                  <div className="flex flex-wrap gap-4 text-sm text-[#a0a0a0]">
                    <span>{activity.date}</span>
                    <span>•</span>
                    <span>{activity.platform}</span>
                    <span>•</span>
                    <span>{activity.hours} hours played</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          // 2024 Diary Table
          <div className="bg-[#0a0a0a] border border-white/10 rounded-xl overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-white/10">
                    <th className="text-left p-4 text-[#a0a0a0]">Date</th>
                    <th className="text-left p-4 text-[#a0a0a0]">Game</th>
                    <th className="text-left p-4 text-[#a0a0a0]">Rating</th>
                    <th className="text-left p-4 text-[#a0a0a0]">Platform</th>
                    <th className="text-left p-4 text-[#a0a0a0]">Hours</th>
                  </tr>
                </thead>
                <tbody>
                  {gamesDiary.map((entry, index) => (
                    <tr
                      key={index}
                      className="border-b border-white/5 hover:bg-[rgba(0,240,255,0.05)] transition-colors"
                    >
                      <td className="p-4 text-[#d0d0d0]">{entry.date}</td>
                      <td className="p-4">
                        <span className="hover:text-[#00f0ff] cursor-pointer transition-colors">
                          {entry.game}
                        </span>
                      </td>
                      <td className="p-4">
                        <div className="flex items-center gap-1">
                          {[...Array(5)].map((_, i) => (
                            <Star
                              key={i}
                              className={`w-3.5 h-3.5 ${
                                i < entry.rating
                                  ? 'fill-[#00f0ff] text-[#00f0ff]'
                                  : 'text-[#a0a0a0]'
                              }`}
                            />
                          ))}
                        </div>
                      </td>
                      <td className="p-4 text-[#d0d0d0]">{entry.platform}</td>
                      <td className="p-4 text-[#d0d0d0]">{entry.hours}h</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Back button */}
      {onBack && (
        <div className="fixed bottom-8 left-8">
          <button
            onClick={onBack}
            className="px-6 py-3 bg-[#0a0a0a] border border-white/10 rounded-full hover:border-[#00f0ff] transition-colors flex items-center gap-2"
          >
            <ChevronLeft className="w-4 h-4" />
            Back to Home
          </button>
        </div>
      )}
    </div>
  );
}
