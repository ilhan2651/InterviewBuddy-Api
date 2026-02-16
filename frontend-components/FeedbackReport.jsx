// src/pages/FeedbackReport.jsx
import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Trophy, TrendingUp, MessageCircle, Zap, ChevronDown, ChevronUp, Home } from 'lucide-react';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Progress from '../components/ui/Progress';
import { getInterviewReport } from '../services/api';

const FeedbackReport = () => {
    const { sessionId } = useParams();
    const navigate = useNavigate();
    const [report, setReport] = useState(null);
    const [expandedQuestions, setExpandedQuestions] = useState([]);
    const [animatedScore, setAnimatedScore] = useState(0);

    useEffect(() => {
        fetchReport();
    }, []);

    const fetchReport = async () => {
        try {
            const response = await getInterviewReport(sessionId);
            setReport(response.data);
            animateScore(response.data.overallScore);
        } catch (error) {
            console.error('Rapor yüklenemedi', error);
        }
    };

    const animateScore = (targetScore) => {
        let current = 0;
        const increment = targetScore / 50;

        const interval = setInterval(() => {
            current += increment;
            if (current >= targetScore) {
                current = targetScore;
                clearInterval(interval);
            }
            setAnimatedScore(Math.round(current));
        }, 20);
    };

    const toggleQuestion = (index) => {
        setExpandedQuestions(prev =>
            prev.includes(index)
                ? prev.filter(i => i !== index)
                : [...prev, index]
        );
    };

    const getScoreColor = (score) => {
        if (score >= 80) return 'text-green-400';
        if (score >= 60) return 'text-yellow-400';
        return 'text-red-400';
    };

    const getGrade = (score) => {
        if (score >= 90) return { grade: 'A+', emoji: '🏆', text: 'Mükemmel!' };
        if (score >= 80) return { grade: 'A', emoji: '🌟', text: 'Harika!' };
        if (score >= 70) return { grade: 'B', emoji: '👍', text: 'İyi!' };
        if (score >= 60) return { grade: 'C', emoji: '👌', text: 'Orta' };
        return { grade: 'D', emoji: '📚', text: 'Gelişmeli' };
    };

    if (!report) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-[#1A1A2E] via-[#252540] to-[#1A1A2E] flex items-center justify-center">
                <div className="text-center">
                    <div className="w-16 h-16 border-4 border-[#A8E6CF] border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                    <p className="text-gray-400">Rapor hazırlanıyor...</p>
                </div>
            </div>
        );
    }

    const scoreInfo = getGrade(report.overallScore);

    return (
        <div className="min-h-screen bg-gradient-to-br from-[#1A1A2E] via-[#252540] to-[#1A1A2E] p-6">
            <div className="max-w-5xl mx-auto">
                {/* Header */}
                <div className="text-center mb-8">
                    <h1 className="text-4xl font-bold text-white mb-2">
                        Mülakat Sonuçları {scoreInfo.emoji}
                    </h1>
                    <p className="text-gray-400">Performansını incele ve gelişim alanlarını keşfet</p>
                </div>

                {/* Overall Score Card */}
                <Card glass className="mb-8 text-center">
                    <div className="mb-6">
                        <div className="inline-block">
                            <div className="relative">
                                <svg className="w-48 h-48 transform -rotate-90">
                                    <circle
                                        cx="96"
                                        cy="96"
                                        r="80"
                                        stroke="rgba(255,255,255,0.1)"
                                        strokeWidth="12"
                                        fill="none"
                                    />
                                    <circle
                                        cx="96"
                                        cy="96"
                                        r="80"
                                        stroke="url(#gradient)"
                                        strokeWidth="12"
                                        fill="none"
                                        strokeDasharray={`${(animatedScore / 100) * 502} 502`}
                                        strokeLinecap="round"
                                        className="transition-all duration-1000"
                                    />
                                    <defs>
                                        <linearGradient id="gradient" x1="0%" y1="0%" x2="100%" y2="100%">
                                            <stop offset="0%" stopColor="#A8E6CF" />
                                            <stop offset="100%" stopColor="#DCD6F7" />
                                        </linearGradient>
                                    </defs>
                                </svg>
                                <div className="absolute inset-0 flex flex-col items-center justify-center">
                                    <span className={`text-5xl font-bold ${getScoreColor(animatedScore)}`}>
                                        {animatedScore}
                                    </span>
                                    <span className="text-gray-400 text-sm mt-1">/ 100</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <h2 className="text-3xl font-bold text-white mb-2">
                        {scoreInfo.grade} - {scoreInfo.text}
                    </h2>
                    <p className="text-gray-400">
                        Tebrikler! Mülakatı başarıyla tamamladın
                    </p>
                </Card>

                {/* Category Scores */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                    <Card glass>
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-12 h-12 rounded-xl bg-[#A8E6CF]/20 flex items-center justify-center">
                                <Zap className="text-[#A8E6CF]" size={24} />
                            </div>
                            <h3 className="font-semibold text-white">Teknik Bilgi</h3>
                        </div>
                        <Progress value={report.technicalScore} color="primary" />
                    </Card>

                    <Card glass>
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-12 h-12 rounded-xl bg-[#DCD6F7]/20 flex items-center justify-center">
                                <MessageCircle className="text-[#DCD6F7]" size={24} />
                            </div>
                            <h3 className="font-semibold text-white">İletişim</h3>
                        </div>
                        <Progress value={report.communicationScore} color="secondary" />
                    </Card>

                    <Card glass>
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-12 h-12 rounded-xl bg-green-500/20 flex items-center justify-center">
                                <TrendingUp className="text-green-400" size={24} />
                            </div>
                            <h3 className="font-semibold text-white">Özgüven</h3>
                        </div>
                        <Progress value={report.confidenceScore} color="success" />
                    </Card>
                </div>

                {/* Questions & Answers Accordion */}
                <Card glass className="mb-8">
                    <h2 className="text-2xl font-bold text-white mb-6 flex items-center gap-2">
                        <Trophy className="text-[#A8E6CF]" size={28} />
                        Soru & Cevap Detayları
                    </h2>

                    <div className="space-y-4">
                        {report.questionAnswers?.map((qa, index) => (
                            <div key={index} className="border border-white/10 rounded-2xl overflow-hidden">
                                <button
                                    onClick={() => toggleQuestion(index)}
                                    className="w-full p-4 bg-white/5 hover:bg-white/10 transition-colors flex items-center justify-between"
                                >
                                    <div className="flex items-center gap-3">
                                        <span className="text-[#A8E6CF] font-bold">#{index + 1}</span>
                                        <span className="text-white font-medium">{qa.question}</span>
                                    </div>
                                    {expandedQuestions.includes(index) ? (
                                        <ChevronUp className="text-gray-400" size={20} />
                                    ) : (
                                        <ChevronDown className="text-gray-400" size={20} />
                                    )}
                                </button>

                                {expandedQuestions.includes(index) && (
                                    <div className="p-6 space-y-4">
                                        <div>
                                            <h4 className="text-sm font-semibold text-gray-400 mb-2">Senin Cevabın:</h4>
                                            <p className="text-white bg-white/5 p-4 rounded-xl">
                                                {qa.userAnswer}
                                            </p>
                                        </div>

                                        <div>
                                            <h4 className="text-sm font-semibold text-gray-400 mb-2">AI Değerlendirmesi:</h4>
                                            <p className="text-gray-300 bg-white/5 p-4 rounded-xl leading-relaxed">
                                                {qa.aiFeedback}
                                            </p>
                                        </div>

                                        <div className="flex items-center gap-2">
                                            <span className="text-sm text-gray-400">Puan:</span>
                                            <span className={`text-lg font-bold ${getScoreColor(qa.score)}`}>
                                                {qa.score}/100
                                            </span>
                                        </div>
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                </Card>

                {/* AI Recommendations */}
                <Card glass className="mb-8">
                    <h2 className="text-2xl font-bold text-white mb-4">
                        🎯 Gelişim Önerileri
                    </h2>
                    <div className="prose prose-invert max-w-none">
                        <ul className="text-gray-300 space-y-2">
                            {report.recommendations?.map((rec, idx) => (
                                <li key={idx} className="flex items-start gap-2">
                                    <span className="text-[#A8E6CF] mt-1">•</span>
                                    <span>{rec}</span>
                                </li>
                            ))}
                        </ul>
                    </div>
                </Card>

                {/* Action Buttons */}
                <div className="flex justify-center gap-4">
                    <Button variant="outline" size="lg" onClick={() => navigate('/dashboard')}>
                        <Home size={20} />
                        Ana Sayfa
                    </Button>
                    <Button variant="primary" size="lg" onClick={() => navigate('/interview/setup')}>
                        Yeni Mülakat Başlat
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default FeedbackReport;
