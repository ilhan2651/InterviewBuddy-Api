// src/pages/InterviewRoom.jsx
import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Mic, MicOff, Send, X, Volume2 } from 'lucide-react';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import { submitAnswer, getCurrentQuestion } from '../services/api';

const InterviewRoom = () => {
    const { sessionId } = useParams();
    const navigate = useNavigate();
    const [currentQuestion, setCurrentQuestion] = useState('');
    const [isRecording, setIsRecording] = useState(false);
    const [audioLevel, setAudioLevel] = useState(0);
    const [questionNumber, setQuestionNumber] = useState(1);
    const [totalQuestions, setTotalQuestions] = useState(10);
    const [isAvatarSpeaking, setIsAvatarSpeaking] = useState(false);
    const canvasRef = useRef(null);
    const audioContextRef = useRef(null);
    const mediaRecorderRef = useRef(null);

    useEffect(() => {
        fetchQuestion();
        initializeAudioContext();
    }, []);

    const fetchQuestion = async () => {
        try {
            const response = await getCurrentQuestion(sessionId);
            setCurrentQuestion(response.data.questionText);
            setQuestionNumber(response.data.questionNumber);
            playAIVoice(response.data.audioUrl);
        } catch (error) {
            console.error('Soru alınamadı', error);
        }
    };

    const playAIVoice = (audioUrl) => {
        const audio = new Audio(audioUrl);
        setIsAvatarSpeaking(true);

        audio.onended = () => {
            setIsAvatarSpeaking(false);
        };

        audio.play();
    };

    const initializeAudioContext = async () => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const audioContext = new AudioContext();
            const analyser = audioContext.createAnalyser();
            const microphone = audioContext.createMediaStreamSource(stream);

            analyser.fftSize = 256;
            microphone.connect(analyser);

            const dataArray = new Uint8Array(analyser.frequencyBinCount);

            const updateWaveform = () => {
                analyser.getByteFrequencyData(dataArray);
                const average = dataArray.reduce((a, b) => a + b) / dataArray.length;
                setAudioLevel(average);

                if (isRecording) {
                    drawWaveform(dataArray);
                }

                requestAnimationFrame(updateWaveform);
            };

            updateWaveform();
            audioContextRef.current = { stream, audioContext, analyser };
        } catch (error) {
            console.error('Audio context hatası', error);
        }
    };

    const drawWaveform = (dataArray) => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const width = canvas.width;
        const height = canvas.height;

        ctx.clearRect(0, 0, width, height);
        ctx.fillStyle = '#1A1A2E';
        ctx.fillRect(0, 0, width, height);

        const barWidth = (width / dataArray.length) * 2.5;
        let x = 0;

        for (let i = 0; i < dataArray.length; i++) {
            const barHeight = (dataArray[i] / 255) * height;

            const gradient = ctx.createLinearGradient(0, height - barHeight, 0, height);
            gradient.addColorStop(0, '#A8E6CF');
            gradient.addColorStop(1, '#DCD6F7');

            ctx.fillStyle = gradient;
            ctx.fillRect(x, height - barHeight, barWidth, barHeight);

            x += barWidth + 1;
        }
    };

    const toggleRecording = () => {
        if (isRecording) {
            stopRecording();
        } else {
            startRecording();
        }
    };

    const startRecording = () => {
        setIsRecording(true);
        const mediaRecorder = new MediaRecorder(audioContextRef.current.stream);
        const audioChunks = [];

        mediaRecorder.ondataavailable = (event) => {
            audioChunks.push(event.data);
        };

        mediaRecorder.onstop = async () => {
            const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
            await submitUserAnswer(audioBlob);
        };

        mediaRecorder.start();
        mediaRecorderRef.current = mediaRecorder;
    };

    const stopRecording = () => {
        setIsRecording(false);
        if (mediaRecorderRef.current) {
            mediaRecorderRef.current.stop();
        }
    };

    const submitUserAnswer = async (audioBlob) => {
        try {
            const formData = new FormData();
            formData.append('audio', audioBlob);
            formData.append('sessionId', sessionId);

            const response = await submitAnswer(formData);

            if (response.data.isComplete) {
                navigate(`/interview/report/${sessionId}`);
            } else {
                setCurrentQuestion(response.data.nextQuestion);
                setQuestionNumber(prev => prev + 1);
                playAIVoice(response.data.audioUrl);
            }
        } catch (error) {
            console.error('Cevap gönderilemedi', error);
        }
    };

    const endInterview = () => {
        navigate(`/interview/report/${sessionId}`);
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-[#1A1A2E] via-[#252540] to-[#1A1A2E] relative overflow-hidden">
            {/* Unity Avatar Placeholder (Bu kısım Unity WebGL entegrasyonu yapıldığında doldurulacak) */}
            <div className="absolute inset-0 flex items-center justify-center">
                <div className="text-center">
                    <div className={`w-64 h-64 rounded-full bg-gradient-to-br from-[#A8E6CF] to-[#DCD6F7] flex items-center justify-center ${isAvatarSpeaking ? 'animate-pulse' : ''}`}>
                        <span className="text-8xl">🤖</span>
                    </div>
                    <p className="text-white mt-4 text-lg">
                        {isAvatarSpeaking ? 'AI Konuşuyor...' : 'Cevabını Bekliyor...'}
                    </p>
                </div>
            </div>

            {/* Progress Bar */}
            <div className="absolute top-6 left-1/2 -translate-x-1/2 w-96">
                <div className="flex justify-between text-sm text-gray-400 mb-2">
                    <span>Soru {questionNumber}/{totalQuestions}</span>
                    <span>{Math.round((questionNumber / totalQuestions) * 100)}%</span>
                </div>
                <div className="h-2 bg-white/10 rounded-full overflow-hidden backdrop-blur-sm">
                    <div
                        className="h-full bg-gradient-to-r from-[#A8E6CF] to-[#DCD6F7] transition-all duration-500"
                        style={{ width: `${(questionNumber / totalQuestions) * 100}%` }}
                    ></div>
                </div>
            </div>

            {/* End Interview Button */}
            <div className="absolute top-6 right-6">
                <Button variant="danger" size="sm" onClick={endInterview}>
                    <X size={16} />
                    Bitir
                </Button>
            </div>

            {/* Bottom Panel */}
            <div className="absolute bottom-0 left-0 right-0 p-6">
                <Card glass className="max-w-4xl mx-auto">
                    {/* Question Display */}
                    <div className="mb-6">
                        <div className="flex items-center gap-2 mb-3">
                            <Volume2 className="text-[#A8E6CF]" size={20} />
                            <h3 className="text-lg font-semibold text-white">Soru:</h3>
                        </div>
                        <p className="text-xl text-gray-200 leading-relaxed">
                            {currentQuestion || 'Soru yükleniyor...'}
                        </p>
                    </div>

                    {/* Waveform Visualization */}
                    <div className="mb-6">
                        <canvas
                            ref={canvasRef}
                            width={800}
                            height={100}
                            className="w-full rounded-2xl"
                        />
                    </div>

                    {/* Control Buttons */}
                    <div className="flex items-center justify-center gap-4">
                        <Button
                            variant={isRecording ? 'danger' : 'primary'}
                            size="lg"
                            onClick={toggleRecording}
                            className="w-48"
                        >
                            {isRecording ? (
                                <>
                                    <MicOff size={20} />
                                    Kaydı Durdur
                                </>
                            ) : (
                                <>
                                    <Mic size={20} />
                                    Kayda Başla
                                </>
                            )}
                        </Button>

                        {!isRecording && (
                            <Button
                                variant="secondary"
                                size="lg"
                                onClick={stopRecording}
                            >
                                <Send size={20} />
                                Cevabı Gönder
                            </Button>
                        )}
                    </div>

                    {/* Recording Indicator */}
                    {isRecording && (
                        <div className="mt-4 text-center">
                            <div className="inline-flex items-center gap-2 px-4 py-2 bg-red-500/20 border border-red-500/30 rounded-full">
                                <span className="w-2 h-2 bg-red-500 rounded-full animate-pulse"></span>
                                <span className="text-red-400 text-sm font-medium">Kaydediliyor...</span>
                            </div>
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
};

export default InterviewRoom;
