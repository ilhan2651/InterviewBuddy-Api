// src/pages/Dashboard.jsx
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlayCircle, TrendingUp, Clock, Award, ChevronRight } from 'lucide-react';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Progress from '../components/ui/Progress';
import { getUserStats, getRecentInterviews } from '../services/api';

const Dashboard = () => {
    const navigate = useNavigate();
    const [stats, setStats] = useState(null);
    const [recentInterviews, setRecentInterviews] = useState([]);

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        try {
            const [statsRes, interviewsRes] = await Promise.all([
                getUserStats(),
                getRecentInterviews()
            ]);
            setStats(statsRes.data);
            setRecentInterviews(interviewsRes.data);
        } catch (error) {
            console.error('Veri yüklenemedi', error);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-[#1A1A2E] via-[#252540] to-[#1A1A2E] p-6">
            {/* Header */}
            <div className="mb-8">
                <h1 className="text-4xl font-bold text-white mb-2">
                    Hoş Geldin Tekrar! 👋
                </h1>
                <p className="text-gray-400">Mülakat pratiğine devam etmeye hazır mısın?</p>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Sol Panel - Recent Interviews */}
                <div className="lg:col-span-1 space-y-4">
                    <h2 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
                        <Clock className="text-[#A8E6CF]" size={24} />
                        Son Mülakatlar
                    </h2>

                    {recentInterviews.map((interview, idx) => (
                        <Card key={idx} glass hover className="cursor-pointer">
                            <div className="flex items-center justify-between mb-2">
                                <h3 className="font-semibold text-white">{interview.role}</h3>
                                <span className={`px-3 py-1 rounded-full text-xs font-medium ${interview.score >= 80 ? 'bg-green-500/20 text-green-400' :
                                        interview.score >= 60 ? 'bg-yellow-500/20 text-yellow-400' :
                                            'bg-red-500/20 text-red-400'
                                    }`}>
                                    {interview.score}/100
                                </span>
                            </div>
                            <p className="text-sm text-gray-400 mb-3">{interview.date}</p>
                            <Progress value={interview.score} color={interview.score >= 80 ? 'success' : interview.score >= 60 ? 'warning' : 'danger'} />
                        </Card>
                    ))}
                </div>

                {/* Orta Panel - Hero CTA */}
                <div className="lg:col-span-1">
                    <Card glass className="h-full flex flex-col justify-center items-center text-center p-8">
                        <div className="w-24 h-24 rounded-full bg-gradient-to-br from-[#A8E6CF] to-[#DCD6F7] flex items-center justify-center mb-6 animate-pulse">
                            <PlayCircle size={48} className="text-[#1A1A2E]" />
                        </div>

                        <h2 className="text-3xl font-bold text-white mb-4">
                            Yeni Mülakat Başlat
                        </h2>
                        <p className="text-gray-400 mb-8">
                            AI destekli gerçekçi mülakat deneyimi yaşa
                        </p>

                        <Button
                            variant="primary"
                            size="lg"
                            onClick={() => navigate('/interview/setup')}
                            className="w-full"
                        >
                            Hadi Başlayalım
                            <ChevronRight size={20} />
                        </Button>
                    </Card>
                </div>

                {/* Sağ Panel - Stats */}
                <div className="lg:col-span-1 space-y-4">
                    <h2 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
                        <TrendingUp className="text-[#DCD6F7]" size={24} />
                        İstatistikler
                    </h2>

                    <Card glass>
                        <div className="flex items-center gap-4 mb-4">
                            <div className="w-12 h-12 rounded-xl bg-[#A8E6CF]/20 flex items-center justify-center">
                                <Award className="text-[#A8E6CF]" size={24} />
                            </div>
                            <div>
                                <p className="text-sm text-gray-400">Toplam Mülakat</p>
                                <p className="text-2xl font-bold text-white">{stats?.totalInterviews || 0}</p>
                            </div>
                        </div>
                    </Card>

                    <Card glass>
                        <h3 className="text-sm font-medium text-gray-400 mb-4">Ortalama Skorlar</h3>
                        <div className="space-y-4">
                            <Progress
                                label="Teknik Bilgi"
                                value={stats?.technicalScore || 0}
                                color="primary"
                            />
                            <Progress
                                label="İletişim"
                                value={stats?.communicationScore || 0}
                                color="secondary"
                            />
                            <Progress
                                label="Özgüven"
                                value={stats?.confidenceScore || 0}
                                color="success"
                            />
                        </div>
                    </Card>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;
