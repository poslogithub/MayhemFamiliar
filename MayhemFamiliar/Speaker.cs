using MayhemFamiliar.QueueManager;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace MayhemFamiliar
{
    internal class Speaker
    {
        private readonly Action<string> _log;
        private SpeechSynthesizer _synthesizer;
        private MediaPlayer _mediaPlayer;
        public Speaker(Action<string> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _synthesizer = new SpeechSynthesizer();
            _mediaPlayer = new MediaPlayer();
        }
        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                _log?.Invoke($"{this.GetType().Name}: 開始");
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (DialogueQueue.Queue.TryDequeue(out string dialogue))
                    {
                        await ProcessDialogue(dialogue);
                    }
                    else
                    {
                        // キューが空なら短い間隔で待機（ブロック）
                        await Task.Delay(100, cancellationToken);
                    }
                }
                _log?.Invoke($"{this.GetType().Name}: キャンセルされました");
            }
            catch (OperationCanceledException)
            {
                _log?.Invoke($"{this.GetType().Name}: キャンセルされました");
            }
            catch (Exception ex)
            {
                _log?.Invoke($"{this.GetType().Name}: エラー発生: {ex.Message}");
            }
        }
        private async Task ProcessDialogue(string dialogue)
        {
            _log?.Invoke($"Processing dialogue: {dialogue}");
            MediaPlayer mediaPlayer = new MediaPlayer();
            var stream = await _synthesizer.SynthesizeTextToStreamAsync(dialogue);
            mediaPlayer.Source = MediaSource.CreateFromStream(stream, "audio/wav");
            var tcs = new TaskCompletionSource<int>();
            mediaPlayer.MediaEnded += (sender, o) => tcs.SetResult(0);
            mediaPlayer.Play();
            await tcs.Task;
        }
    }
}
