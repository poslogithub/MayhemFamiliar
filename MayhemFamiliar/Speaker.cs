using MayhemFamiliar.QueueManager;
using System;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal class Speaker
    {
        private SpeechSynthesizer _synthesizer;
        public Speaker()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }
        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                Logger.Instance.Log($"{this.GetType().Name}: 開始");
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (DialogueQueue.Queue.TryDequeue(out string dialogue))
                    {
                        ProcessDialogue(dialogue);
                    }
                    else
                    {
                        // キューが空なら短い間隔で待機（ブロック）
                        await Task.Delay(100, cancellationToken);
                    }
                }
                Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
            }
            catch (OperationCanceledException)
            {
                Logger.Instance.Log($"{this.GetType().Name}: キャンセルされました");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"{this.GetType().Name}: エラー発生: {ex.Message}");
            }
        }
        private void ProcessDialogue(string dialogue)
        {
            Logger.Instance.Log($"{this.GetType().Name}: ダイアログを処理: {dialogue}", LogLevel.Debug);
            _synthesizer.Speak(dialogue);
        }
    }
}
